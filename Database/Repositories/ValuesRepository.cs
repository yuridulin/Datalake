using Datalake.Database.Constants;
using Datalake.Database.Extensions;
using Datalake.Database.Functions;
using Datalake.Database.InMemory.Stores;
using Datalake.Database.InMemory.Stores.Derived;
using Datalake.Database.Models;
using Datalake.Database.Tables;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Tags;
using Datalake.PublicApi.Models.Values;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Datalake.Database.Repositories;

/// <summary>
/// Репозиторий для работы со значениями тегов
/// </summary>
public class ValuesRepository(
	DatalakeContext db,
	DatalakeCachedTagsStore cachedTagsStore,
	DatalakeCurrentValuesStore valuesStore,
	ILogger<ValuesRepository> logger)
{
	#region API

	/// <summary>
	/// Получение значений по списку запрошенных тегов
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="requests">Список запрошенных тегов с настройками получения</param>
	/// <returns>Список ответов со значениями тегов</returns>
	public async Task<List<ValuesResponse>> GetValuesAsync(
		UserAuthInfo user,
		ValuesRequest[] requests)
	{
		var currentState = cachedTagsStore.State;

		var trustedRequests = requests
			.Select(request =>
			{
				var identifiers = request.TagsId?.ToHashSet() ?? [];
				var guids = request.Tags?.ToHashSet() ?? [];

				var foundTags = currentState.CachedTags
					.Where(tag => identifiers.Contains(tag.Id) || guids.Contains(tag.Guid))
					.Select(x => new ValuesTrustedRequest.TagSettings
					{
						Guid = x.Guid,
						Id = x.Id,
						Name = x.Name,
						Resolution = x.Resolution,
						ScalingCoefficient = x.ScalingCoefficient,
						SourceId = x.SourceId,
						SourceType = x.SourceType,
						Type = x.Type,
						IsDeleted = x.IsDeleted,
						Result = 
							!AccessChecks.HasAccessToTag(user, AccessType.Viewer, x.Id) ? ValueResult.NoAccess
							: x.IsDeleted ? ValueResult.IsDeleted
							: ValueResult.Ok,
					})
					.ToArray();

				// заглушки для тегов, которые мы не нашли в системе
				var notFoundById = identifiers
					.Except(foundTags.Select(t => t.Id))
					.Select(id => new ValuesTrustedRequest.TagSettings
					{
						Id = id,
						Guid = Guid.Empty,
						Name = string.Empty,
						Resolution = TagResolution.NotSet,
						ScalingCoefficient = 1,
						SourceId = 0,
						SourceType = SourceType.NotSet,
						Type = TagType.String,
						IsDeleted = false,
						Result = ValueResult.NotFound
					});

				var notFoundByGuid = guids
					.Except(foundTags.Select(t => t.Guid))
					.Select(guid => new ValuesTrustedRequest.TagSettings
					{
						Id = 0,
						Guid = guid,
						Name = string.Empty,
						Resolution = TagResolution.NotSet,
						ScalingCoefficient = 1,
						SourceId = 0,
						SourceType = SourceType.NotSet,
						Type = TagType.String,
						IsDeleted = false,
						Result = ValueResult.NotFound
					});

				return new ValuesTrustedRequest
				{
					RequestKey = request.RequestKey,
					Time = new ValuesTrustedRequest.TimeSettings
					{
						Old = request.Old,
						Young = request.Young,
						Exact = request.Exact,
					},
					Resolution = request.Resolution,
					Func = request.Func,
					Tags = foundTags
						.Concat(notFoundById)
						.Concat(notFoundByGuid)
						.ToArray(),
				};
			})
			.ToArray();

		return await Measures.MeasureAsync(() => ProtectedGetValuesAsync(db, trustedRequests), logger, nameof(ProtectedGetValuesAsync));
	}

	/// <summary>
	/// Запись очередных новых значений, собранных из источников
	/// </summary>
	/// <param name="requests">Список данных к записи, непроверенный</param>
	/// <returns></returns>
	public async Task WriteCollectedValuesAsync(IEnumerable<ValueWriteRequest> requests)
	{
		var currentState = cachedTagsStore.State;

		List<TagHistory> records = [];

		foreach (var request in requests)
		{
			TagCacheInfo? tag = null;

			if (request.Id.HasValue)
				currentState.CachedTagsById.TryGetValue(request.Id.Value, out tag);
			else if (request.Guid.HasValue)
				currentState.CachedTagsByGuid.TryGetValue(request.Guid.Value, out tag);

			if (tag == null)
				continue;

			var record = CreateFrom(tag, request);

			// проверка на уникальность (новизну)
			if (!valuesStore.IsNew(record.TagId, record))
				continue;

			records.Add(record);
		}

		var uniqueRecords = records.GroupBy(x => new { x.TagId, x.Date })
			.Select(g => g.First())
			.ToList();

		var writeResult = await DatabaseValues.WriteAsync(db, logger, uniqueRecords);
		if (writeResult)
		{
			// обновление в кэше текущих данных
			foreach (var record in uniqueRecords)
				valuesStore.TryUpdate(record.TagId, record);
		}
	}

	/// <summary>
	/// Запись новых значений для указанных тегов
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="requests">Список тегов с новыми значениями</param>
	/// <returns>Список записанных значений</returns>
	public async Task<List<ValuesTagResponse>> WriteManualValuesAsync(
		UserAuthInfo user,
		ValueWriteRequest[] requests)
	{
		var currentState = cachedTagsStore.State;

		List<ValuesTagResponse> responses = [];
		List<TagHistory> recordsToWrite = [];

		foreach (var request in requests)
		{
			TagCacheInfo? tag = null;
			var date = request.Date ?? DateFormats.GetCurrentDateTime();

			if (request.Id.HasValue)
				currentState.CachedTagsById.TryGetValue(request.Id.Value, out tag);
			else if (request.Guid.HasValue)
				currentState.CachedTagsByGuid.TryGetValue(request.Guid.Value, out tag);

			if (tag == null)
			{
				responses.Add(new ValuesTagResponse
				{
					Result = ValueResult.NotFound,
					Id = request.Id ?? 0,
					Guid = request.Guid ?? Guid.Empty,
					Name = string.Empty,
					Type = TagType.String,
					Resolution = TagResolution.NotSet,
					SourceType = SourceType.NotSet,
					Values = [
						new ValueRecord
						{
							Date = date,
							DateString = date.ToString(DateFormats.HierarchicalWithMilliseconds),
							Quality = TagQuality.Unknown,
							Value = null,
						}
					]
				});
				continue;
			}

			var record = CreateFrom(tag, request);
			record.Date = date;

			var result =
				tag.IsDeleted ? ValueResult.IsDeleted
				: tag.SourceType != SourceType.Manual ? ValueResult.NotManual
				: !AccessChecks.HasAccessToTag(user, AccessType.Editor, tag.Id) ? ValueResult.NoAccess
				: ValueResult.Ok;

			responses.Add(new ValuesTagResponse
			{
				Result = result,
				Id = tag.Id,
				Guid = tag.Guid,
				Name = tag.Name,
				Type = tag.Type,
				Resolution = tag.Resolution,
				SourceType = tag.SourceType,
				Values = [
					new ValueRecord
					{
						Date = record.Date,
						DateString = record.Date.ToString(DateFormats.HierarchicalWithMilliseconds),
						Quality = record.Quality,
						Value = record.GetTypedValue(tag.Type),
					}
				]
			});

			if (result == ValueResult.Ok)
				recordsToWrite.Add(record);
		}

		var uniqueRecords = recordsToWrite.GroupBy(x => new { x.TagId, x.Date })
			.Select(g => g.First())
			.ToList();

		var writeResult = await DatabaseValues.WriteAsync(db, logger, uniqueRecords);
		if (writeResult)
		{
			// обновление в кэше текущих данных
			foreach (var record in uniqueRecords)
				valuesStore.TryUpdate(record.TagId, record);
		}
		else
		{
			foreach (var response in responses)
			{
				if (response.Result == ValueResult.Ok)
					response.Result = ValueResult.UnknownError;
			}
		}

		return responses;
	}

	#endregion API

	#region Чтение

	internal async Task<List<ValuesResponse>> ProtectedGetValuesAsync(
		DatalakeContext db, ValuesTrustedRequest[] trustedRequests)
	{
		List<ValuesResponse> responses = [];

		var sqlScopes = trustedRequests
			.GroupBy(x => x.Time)
			.Select(g => new ValuesSqlScope
			{
				Settings = g.Key,
				Requests = g.ToArray(),
				Keys = g.Select(x => x.RequestKey).ToArray(),
				TagsId = g.SelectMany(r => r.Tags).Where(r => r.Result == ValueResult.Ok).Select(r => r.Id).ToArray(),
			})
			.ToArray();

		foreach (var scope in sqlScopes)
		{
			TagHistory[] databaseValues;

			// получение среза
			if (!scope.Settings.Old.HasValue && !scope.Settings.Young.HasValue)
			{
				Dictionary<int, TagHistory?> databaseValuesById;

				if (scope.Settings.Exact.HasValue)
				{
					databaseValues = await DatabaseValues.ReadExactAsync(db, logger, scope, scope.Settings.Exact.Value);
					databaseValuesById = databaseValues.ToDictionary(x => x.TagId)!;
				}
				else
				{
					// Если не указывается ни одна дата, выполняется получение текущих значений. Не убирать!
					databaseValuesById = valuesStore.GetByIdentifiers(scope.TagsId);
					scope.Settings.Exact = DateFormats.GetCurrentDateTime();
				}

				foreach (var request in scope.Requests)
				{
					List<ValuesTagResponse> tags = [];
					foreach (var tag in request.Tags)
					{
						ValuesTagResponse tagResponse = new()
						{
							Result = tag.Result,
							Id = tag.Id,
							Guid = tag.Guid,
							Name = tag.Name,
							Type = tag.Type,
							Resolution = tag.Resolution,
							SourceType = tag.SourceType,
							Values = [],
						};

						if (tag.Result == ValueResult.Ok)
						{
							if (!databaseValuesById.TryGetValue(tag.Id, out var value) || value == null)
							{
								tag.Result = ValueResult.ValueNotFound;
								value = new TagHistory { TagId = tag.Id, Date = scope.Settings.Exact.Value, Quality = TagQuality.Bad_NoValues };
							}

							var tagValue = new ValueRecord
							{
								Date = value.Date,
								DateString = value.Date.ToString(DateFormats.HierarchicalWithMilliseconds),
								Quality = value.Quality,
								Value = value.GetTypedValue(tag.Type),
							};

							tagResponse.Values = [tagValue];
						}

						tags.Add(tagResponse);
					}

					var response = new ValuesResponse
					{
						RequestKey = request.RequestKey,
						Tags = tags,
					};

					responses.Add(response);
				}
			}

			// получение истории
			else
			{
				scope.Settings.Young ??= DateFormats.GetCurrentDateTime();
				scope.Settings.Old ??= scope.Settings.Young;

				databaseValues = await DatabaseValues.ReadRangeAsync(db, logger, scope, scope.Settings.Old.Value, scope.Settings.Young.Value);

				foreach (var request in scope.Requests)
				{
					var response = new ValuesResponse
					{
						RequestKey = request.RequestKey,
						Tags = [],
					};

					var tagsResponses = new List<ValuesTagResponse>();
					var requestIdentifiers = request.Tags.Select(t => t.Id).ToArray();
					var requestValues = databaseValues.Where(x => requestIdentifiers.Contains(x.TagId));

					foreach (var tag in request.Tags)
					{
						var tagResponse = new ValuesTagResponse
						{
							Result = tag.Result,
							Guid = tag.Guid,
							Id = tag.Id,
							Name = tag.Name,
							Type = tag.Type,
							Resolution = tag.Resolution,
							SourceType = tag.SourceType,
							Values = [],
						};
						var tagValues = requestValues.Where(x => x.TagId == tag.Id).ToList();

						// если у нас не Ok, то тега нет, или нет доступа к нему
						// так или иначе, значения нас уже не интересуют
						if (tagResponse.Result == ValueResult.Ok)
						{
							// так как при получении истории мы делаем locf значений до начала диапазона, у нас должно быть минимум одно значение
							// если ноль - условие не корректно или тега вообще не существовало
							if (tagValues.Count == 0)
							{
								tagResponse.Result = ValueResult.ValueNotFound;
								tagResponse.Values = [
									new()
									{
										Date = scope.Settings.Old.Value,
										DateString = scope.Settings.Old.Value.ToString(DateFormats.HierarchicalWithMilliseconds),
										Quality = TagQuality.Bad_NoValues,
										Value = 0,
									}
								];
							}
							if (request.Resolution != null && request.Resolution > 0)
							{
								tagValues = StretchByResolution(tagValues, scope.Settings.Old.Value, scope.Settings.Young.Value, request.Resolution.Value);
							}

							if (tag.Type == TagType.Number && request.Func != AggregationFunc.List)
							{
								var numericValues = tagValues
									.Where(x => x.Quality == TagQuality.Good || x.Quality == TagQuality.Good_ManualWrite)
									.Select(x => x.GetTypedValue(TagType.Number) as float?);

								float? value = 0;
								try
								{
									switch (request.Func)
									{
										case AggregationFunc.Sum:
											value = numericValues.Sum();
											break;

										case AggregationFunc.Avg:
											value = numericValues.Average();
											break;

										case AggregationFunc.Min:
											value = numericValues.Min();
											break;

										case AggregationFunc.Max:
											value = numericValues.Max();
											break;
									}

									tagResponse.Values = [
										new() {
											Date = scope.Settings.Old.Value,
											DateString = scope.Settings.Old.Value.ToString(DateFormats.HierarchicalWithMilliseconds),
											Quality = TagQuality.Good,
											Value = value,
										}
									];
								}
								catch
								{
								}
							}
							else
							{
								tagResponse.Values = [
									..tagValues
									.Select(x => new ValueRecord
									{
										Date = x.Date,
										DateString = x.Date.ToString(DateFormats.HierarchicalWithMilliseconds),
										Quality = x.Quality,
										Value = x.GetTypedValue(tag.Type),
									})
									.OrderBy(x => x.Date)
								];
							}
						}

						tagsResponses.Add(tagResponse);
					}

					response.Tags = tagsResponses;
					responses.Add(response);
				}
			}
		}

		return responses;
	}

	private const int _stretchLimit = 100000;

	private static List<TagHistory> StretchByResolution(
		List<TagHistory> valuesByChange,
		DateTime old,
		DateTime young,
		TagResolution resolution)
	{
		var timeRange = (young - old).TotalMilliseconds;
		var continuous = new List<TagHistory>();
		DateTime stepDate = old;
		int step = 0;

		do
		{
			var value = valuesByChange
				.Where(x => x.Date <= stepDate)
				.OrderByDescending(x => x.Date)
				.FirstOrDefault();

			if (value != null)
			{
				if (value.Date != stepDate)
				{
					continuous.Add(new TagHistory
					{
						TagId = value.TagId,
						Date = stepDate,
						Text = value.Text,
						Number = value.Number,
						Quality = value.Quality.GetLOCFValue(),
					});
				}
				else
				{
					continuous.Add(value);
				}
			}

			stepDate = stepDate.AddByResolution(resolution);
			step++;
		}
		while (stepDate <= young && step < _stretchLimit);

		return continuous;
	}

	#endregion Чтение

	#region Запись

	internal static TagHistory CreateFrom(TagCacheInfo tag, ValueWriteRequest request)
	{
		return CreateTagHistory(
			tag.Type,
			tag.Id,
			tag.Resolution,
			tag.ScalingCoefficient,
			request.Date,
			request.Value,
			request.Quality);
	}

	internal static TagHistory CreateFrom(ValueTrustedWriteRequest request)
	{
		return CreateTagHistory(
			request.Tag.Type,
			request.Tag.Id,
			request.Tag.Resolution,
			request.Tag.ScalingCoefficient,
			request.Date,
			request.Value,
			request.Quality);
	}

	private static TagHistory CreateTagHistory(
			TagType tagType,
			int tagId,
			TagResolution frequency,
			float scalingCoefficient,
			DateTime? date,
			object? value,
			TagQuality? quality)
	{
		var history = new TagHistory
		{
			Date = (date ?? DateFormats.GetCurrentDateTime()).RoundByResolution(frequency),
			Text = null,
			Number = null,
			Quality = quality ?? TagQuality.Unknown,
			TagId = tagId,
		};

		if (value == null)
			return history;

		string text = value.ToString()!;

		switch (tagType)
		{
			case TagType.String:
				history.Text = text;
				break;

			case TagType.Number:
				if (double.TryParse(text ?? "x", NumberStyles.Float, CultureInfo.InvariantCulture, out double dValue))
				{
					float number = (float)dValue;

					if (scalingCoefficient != 1)
					{
						history.Number = number * scalingCoefficient;
					}
					else
					{
						history.Number = number;
					}
				}
				break;

			case TagType.Boolean:
				history.Number = text == Values.One || text == Values.True ? 1 : 0;
				break;
		}

		return history;
	}

	#endregion Запись
}

internal class ValuesSqlScope
{
	internal required ValuesTrustedRequest.TimeSettings Settings { get; set; }
	internal required ValuesTrustedRequest[] Requests { get; set; }
	internal required string[] Keys { get; set; }
	internal required int[] TagsId { get; set; }
}

internal static class DatabaseValues
{
	#region Чтение

	internal static async Task<TagHistory[]> ReadRangeAsync(
		DatalakeContext db,
		ILogger logger,
		ValuesSqlScope scope,
		DateTime old,
		DateTime young)
	{
		if (scope.TagsId.Length == 0)
			return [];

		return await Measures.MeasureAsync(async () =>
		{
			var values = await db.QueryToArrayAsync<TagHistory>(ReadRangeSql
				.MapIdentifiers(scope.TagsId)
				.MapDate("@old", old)
				.MapDate("@young", young));

			return values;
		},
		logger, nameof(ReadRangeAsync), ReadQueryOperationName, scope);
	}

	internal static async Task<TagHistory[]> ReadAllCurrentAsync(
		DatalakeContext db,
		ILogger logger)
	{
		return await Measures.MeasureAsync(async () =>
		{
			return await db.QueryToArrayAsync<TagHistory>(ReadAllCurrentSql);
		},
		logger, nameof(ReadAllCurrentAsync), ReadQueryOperationName);
	}

	internal static async Task<TagHistory[]> ReadCurrentAsync(
		DatalakeContext db,
		ILogger logger,
		ValuesSqlScope scope)
	{
		if (scope.TagsId.Length == 0)
			return [];

		return await Measures.MeasureAsync(async () =>
		{
			return await db.QueryToArrayAsync<TagHistory>(ReadCurrentSql
				.MapIdentifiers(scope.TagsId));
		},
		logger, nameof(ReadCurrentAsync), ReadQueryOperationName, scope);
	}

	internal static async Task<TagHistory[]> ReadExactAsync(
		DatalakeContext db,
		ILogger logger,
		ValuesSqlScope scope,
		DateTime exactDate)
	{
		if (scope.TagsId.Length == 0)
			return [];

		return await Measures.MeasureAsync(async () =>
		{
			return await db.QueryToArrayAsync<TagHistory>(ReadExactSql
				.MapIdentifiers(scope.TagsId)
				.MapDate("@old", exactDate));
		},
		logger, nameof(ReadExactAsync), ReadQueryOperationName, scope);
	}

	private const string ReadQueryOperationName = "query.read";

	/// <summary>
	/// Параметры: нет
	/// </summary>
	private const string ReadAllCurrentSql = @"
		SELECT
			t.""Id"" AS ""TagId"",
			CASE WHEN h.""Date"" IS NULL THEN Now() ELSE h.""Date"" END AS ""Date"",
			h.""Text"",
			h.""Number"",
			CASE WHEN h.""Quality"" IS NULL THEN 8 ELSE h.""Quality"" END AS ""Quality""
		FROM ""Tags"" t
		LEFT JOIN (
			SELECT DISTINCT ON (""TagId"") *
			FROM public.""TagsHistory""
			ORDER BY ""TagId"", ""Date"" DESC) AS h ON t.""Id"" = h.""TagId""";

	/// <summary>
	/// Параметры: теги
	/// </summary>
	private const string ReadCurrentSql = @"
		SELECT DISTINCT ON (""TagId"") *
		FROM public.""TagsHistory""
		WHERE ""TagId"" IN (@tags)
		ORDER BY ""TagId"", ""Date"" DESC;";

	/// <summary>
	/// Параметры: теги, дата начала
	/// </summary>
	private const string ReadExactSql = @"
		SELECT DISTINCT ON (""TagId"") *
		FROM public.""TagsHistory""
		WHERE
			""TagId"" IN (@tags)
			AND ""Date"" <= '@old'
		ORDER BY ""TagId"", ""Date"" DESC";

	/// <summary>
	/// Параметры: теги, дата начала, дата конца
	/// </summary>
	private const string ReadRangeSql = $@"
		SELECT * FROM (
			{ReadExactSql}
		) AS valuesBefore
		UNION ALL
		SELECT *
		FROM public.""TagsHistory""
		WHERE
			""TagId"" IN (@tags)
			AND ""Date"" > '@old'
			AND ""Date"" <= '@young';";

	#endregion Чтение

	#region Запись

	/// <summary>
	/// Запись значений в партицию через временную таблицу
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="logger">Логгер</param>
	/// <param name="records">Список записей для ввода</param>
	internal static async Task<bool> WriteAsync(
		DatalakeContext db,
		ILogger logger,
		List<TagHistory> records)
	{
		if (records.Count == 0)
			return true;

		return await Measures.MeasureAsync(async () =>
		{
			using var transaction = await db.BeginTransactionAsync();

			try
			{
				await db.ExecuteAsync(CreateTempTableForWrite);
				await db.BulkCopyAsync(BulkCopyOptions, records);
				await db.ExecuteAsync(WriteSql);

				await transaction.CommitAsync();
				return true;
			}
			catch (Exception e)
			{
				logger.LogError(e, "Не удалось записать данные");
				await transaction.RollbackAsync();
				return false;
			}
		}, logger, nameof(WriteAsync), WriteQueryOperationName);
	}

	private const string WriteQueryOperationName = "query.write";

	private const string TempTableForWrite = "TagsHistoryState";

	private static readonly BulkCopyOptions BulkCopyOptions = new() { TableName = TempTableForWrite, BulkCopyType = BulkCopyType.ProviderSpecific, };

	private const string CreateTempTableForWrite = $@"
		CREATE TEMPORARY TABLE ""{TempTableForWrite}"" (LIKE public.""TagsHistory"" EXCLUDING INDEXES)
		ON COMMIT DROP;";

	private const string WriteSql = $@"
		INSERT INTO public.""TagsHistory""(
			""TagId"", ""Date"", ""Text"", ""Number"", ""Quality""
		)
		SELECT ""TagId"", ""Date"", ""Text"", ""Number"", ""Quality""
			FROM ""{TempTableForWrite}""
		ON CONFLICT (""TagId"", ""Date"") DO UPDATE
			SET ""Text""   = EXCLUDED.""Text"",
				""Number"" = EXCLUDED.""Number"",
				""Quality""= EXCLUDED.""Quality"";";

	#endregion
}

/// <summary>
/// Работа с преобразованными значениями
/// </summary>
public static class DatabaseAggregation
{
	/// <summary>
	/// Расчет средневзвешенных и взвешенных сумм по тегам. Взвешивание по секундам
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="logger">Логгер</param>
	/// <param name="identifiers">Идентификаторы тегов</param>
	/// <param name="moment">Момент времени, относительно которого определяется прошедший период</param>
	/// <param name="period">Размер прошедшего периода</param>
	/// <returns>По одному значению на каждый тег</returns>
	/// <exception cref="ForbiddenException">Неподдерживаемый период</exception>
	public static async Task<TagAggregationWeightedValue[]> GetWeightedAggregatedValuesAsync(
		DatalakeContext db,
		ILogger logger,
		int[] identifiers,
		DateTime? moment = null,
		AggregationPeriod period = AggregationPeriod.Hour)
	{
		if (identifiers.Length == 0)
			return [];

		return await Measures.MeasureAsync(async () =>
		{
			// Задаем входные параметры
			var now = moment ?? DateFormats.GetCurrentDateTime();

			DateTime periodStart, periodEnd;
			switch (period)
			{
				case AggregationPeriod.Minute:
					periodEnd = now.RoundByResolution(TagResolution.Minute);
					periodStart = periodEnd.AddMinutes(-1);
					break;

				case AggregationPeriod.Hour:
					periodEnd = now.RoundByResolution(TagResolution.Hour);
					periodStart = periodEnd.AddHours(-1);
					break;

				case AggregationPeriod.Day:
					periodEnd = now.RoundByResolution(TagResolution.Day);
					periodStart = periodEnd.AddDays(-1);
					break;

				default:
					throw new ForbiddenException("задан неподдерживаемый период");
			}

			var source = db.TagsHistory.Where(tag => identifiers.Contains(tag.TagId));

			// CTE: Последнее значение перед началом периода
			var historyBefore =
				from raw in source
				where raw.Date <= periodStart
				select new
				{
					raw.TagId,
					Date = periodStart, // Принудительная установка даты
					raw.Number,
					Order = Sql.Ext.RowNumber()
						.Over()
						.PartitionBy(raw.TagId)
						.OrderByDesc(raw.Date)
						.ToValue()
				} into historyBeforeTemp
				where historyBeforeTemp.Order == 1
				select new
				{
					historyBeforeTemp.TagId,
					historyBeforeTemp.Date,
					historyBeforeTemp.Number
				};

			// CTE: Значения в пределах периода
			var historyBetween =
				from raw in source
				where
					raw.Date > periodStart &&
					raw.Date <= periodEnd
				select new
				{
					raw.TagId,
					raw.Date,
					raw.Number
				};

			// CTE: Объединение двух выборок (UNION ALL)
			var history = historyBefore.Concat(historyBetween);

			// CTE: Добавление следующей даты, чтобы относительно ее посчитать длительность актуальности значения
			var historyWithNext =
				from h in history
				select new
				{
					h.TagId,
					h.Date,
					h.Number,
					NextDate = Sql.Ext.Lead(h.Date, 1, periodEnd)
						.Over()
						.PartitionBy(h.TagId)
						.OrderBy(h.Date)
						.ToValue()
				};

			// CTE: Вычисление длительности действия каждого значения
			var weighted =
				from h in historyWithNext
				select new
				{
					h.TagId,
					h.Number,
					Weight = Sql.DateDiff(Sql.DateParts.Second, h.Date, h.NextDate),
				};

			// Финальный запрос - применяем веса
			var result =
				from w in weighted
				group w by w.TagId into g
				select new TagAggregationWeightedValue
				{
					TagId = g.Key,
					Date = periodEnd,
					SumOfWeights = g.Sum(x => x.Weight) ?? 0,
					SumValuesWithWeights = g.Sum(x => x.Number * x.Weight) ?? 0,
				};

			// Выполнение запроса
			var aggregated = await result.ToArrayAsync();

			return aggregated;
		},
		logger, nameof(GetWeightedAggregatedValuesAsync), AggregatedQueryOperationName, new { identifiers, moment, period });
	}

	private const string AggregatedQueryOperationName = "query.agg";
}