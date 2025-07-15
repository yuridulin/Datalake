using Datalake.Database.Attributes;
using Datalake.Database.Extensions;
using Datalake.Database.Functions;
using Datalake.Database.InMemory;
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

namespace Datalake.Database.Repositories;

/// <summary>
/// Репозиторий для работы со значениями тегов
/// </summary>
public class ValuesRepository(
	DatalakeDataStore dataStore,
	DatalakeCurrentValuesStore valuesStore,
	ILogger<ValuesRepository> logger)
{
	#region API

	/// <summary>
	/// Получение значений по списку запрошенных тегов
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="requests">Список запрошенных тегов с настройками получения</param>
	/// <returns>Список ответов со значениями тегов</returns>
	public async Task<List<ValuesResponse>> GetValuesAsync(
		DatalakeContext db,
		UserAuthInfo user,
		ValuesRequest[] requests)
	{
		var currentState = dataStore.State;

		var trustedRequests = requests
			.Select(request =>
			{
				var identifiers = request.TagsId?.ToHashSet() ?? [];
				var guids = request.Tags?.ToHashSet() ?? [];

				var cachedTags = currentState.CachesTags
					.Where(tag => !tag.IsDeleted)
					.Where(tag => identifiers.Contains(tag.Id) || guids.Contains(tag.Guid))
					.Where(tag => AccessChecks.HasAccessToTag(user, AccessType.Viewer, tag.Id))
					.ToArray();

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
					Tags = cachedTags,
				};
			})
			.ToArray();

		return await Measures.Measure(() => ProtectedGetValuesAsync(db, trustedRequests), logger, nameof(ProtectedGetValuesAsync));
	}

	/// <summary>
	/// Запись очередных новых значений, собранных из
	/// </summary>
	/// <param name="db"></param>
	/// <param name="requests"></param>
	/// <returns></returns>
	public async Task WriteCollectedValuesAsync(DatalakeContext db, IEnumerable<ValueWriteRequest> requests)
	{
		var currentState = dataStore.State;

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

			var record = TagHistoryExtension.CreateFrom(tag, request);

			// проверка на уникальность (новизну)
			if (!valuesStore.IsNew(record.TagId, record))
				continue;

			records.Add(record);
		}

		await ProtectedWriteValuesAsync(db, records);
	}

	/// <summary>
	/// Запись новых значений для указанных тегов
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="requests">Список тегов с новыми значениями</param>
	/// <returns>Список записанных значений</returns>
	public async Task<List<ValuesTagResponse>> WriteManualValuesAsync(
		DatalakeContext db,
		UserAuthInfo user,
		ValueWriteRequest[] requests)
	{
		var currentState = dataStore.State;

		List<ValuesTagResponse> responses = [];
		List<TagHistory> recordsToWrite = [];

		foreach (var request in requests)
		{
			TagCacheInfo? tag = null;

			if (request.Id.HasValue)
				currentState.CachedTagsById.TryGetValue(request.Id.Value, out tag);
			else if (request.Guid.HasValue)
				currentState.CachedTagsByGuid.TryGetValue(request.Guid.Value, out tag);

			if (tag == null || tag.SourceType != SourceType.Manual || !AccessChecks.HasAccessToTag(user, AccessType.Editor, tag.Id))
				continue;

			var record = TagHistoryExtension.CreateFrom(tag, request);
			record.Date = request.Date ?? DateFormats.GetCurrentDateTime();

			recordsToWrite.Add(record);

			responses.Add(new ValuesTagResponse
			{
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
		}

		await ProtectedWriteValuesAsync(db, recordsToWrite);

		return responses;
	}

	#endregion

	#region Чтение

	internal async Task<List<ValuesResponse>> ProtectedGetValuesAsync(
		DatalakeContext db, ValuesTrustedRequest[] trustedRequests)
	{
		List<ValuesResponse> responses = [];

		var groups = trustedRequests
			.GroupBy(x => x.Time)
			.Select(g => new
			{
				Settings = g.Key,
				Requests = g.ToArray(),
				TagsId = g.SelectMany(r => r.Tags).Select(r => r.Id).ToArray(),
			})
			.ToArray();

		foreach (var group in groups)
		{
			TagHistory[] databaseValues;

			if (!group.Settings.Old.HasValue && !group.Settings.Young.HasValue)
			{
				Dictionary<int, TagHistory?> databaseValuesById;
				DateTime date;

				if (group.Settings.Exact.HasValue)
				{
					databaseValues = await ProtectedReadLastValuesBeforeDateAsync(db, group.TagsId, group.Settings.Exact.Value);
					databaseValuesById = databaseValues.ToDictionary(x => x.TagId)!;

					date = group.Settings.Exact.Value;
				}
				else
				{
					// Если не указывается ни одна дата, выполняется получение текущих значений. Не убирать!
					databaseValuesById = valuesStore.GetByIdentifiers(group.TagsId);
					date = DateFormats.GetCurrentDateTime();
				}

				foreach (var request in group.Requests)
				{
					List<ValuesTagResponse> tags = [];
					foreach (var tag in request.Tags)
					{
						if (!databaseValuesById.TryGetValue(tag.Id, out var value) || value == null)
						{
							value = new TagHistory { TagId = tag.Id, Date = date, Quality = TagQuality.Bad_NoValues };
						}

						var tagValue = new ValueRecord
						{
							Date = value.Date,
							DateString = value.Date.ToString(DateFormats.HierarchicalWithMilliseconds),
							Quality = value.Quality,
							Value = value.GetTypedValue(tag.Type),
						};

						var tagResponse = new ValuesTagResponse
						{
							Id = tag.Id,
							Guid = tag.Guid,
							Name = tag.Name,
							Type = tag.Type,
							Resolution = tag.Resolution,
							SourceType = tag.SourceType,
							Values = [tagValue],
						};

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
			else
			{
				DateTime
					young = group.Settings.Young ?? DateFormats.GetCurrentDateTime(),
					old = group.Settings.Old ?? young.Date;

				databaseValues = await ProtectedReadValuesAsync(db, group.TagsId, old, young);

				foreach (var request in group.Requests)
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
							Guid = tag.Guid,
							Id = tag.Id,
							Name = tag.Name,
							Type = tag.Type,
							Resolution = tag.Resolution,
							SourceType = tag.SourceType,
							Values = [],
						};
						var tagValues = requestValues.Where(x => x.TagId == tag.Id).ToList();

						if (tagValues.Count == 0)
						{
							tagResponse.Values = [
								new()
								{
									Date = old,
									DateString = old.ToString(DateFormats.HierarchicalWithMilliseconds),
									Quality = TagQuality.Bad_NoValues,
									Value = 0,
								}
							];
						}
						else
						{
							if (request.Resolution != null && request.Resolution > 0)
							{
								tagValues = StretchByResolution(tagValues, old, young, request.Resolution.Value);
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
											Date = old,
											DateString = old.ToString(DateFormats.HierarchicalWithMilliseconds),
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

	internal static async Task<TagHistory[]> ProtectedReadValuesAsync(
		DatalakeContext db,
		int[] identifiers,
		DateTime old,
		DateTime young)
	{
		var values = await db.QueryToArrayAsync<TagHistory>(ReadBetweenDates
			.MapIdentifiers(identifiers)
			.MapDate("@old", old)
			.MapDate("@young", young));

		return values;
	}

	internal static async Task<TagHistory[]> ProtectedReadAllLastValuesAsync(
		DatalakeContext db)
	{
		return await db.QueryToArrayAsync<TagHistory>(ReadAllLast);
	}

	internal static async Task<TagHistory[]> ProtectedReadLastValuesAsync(
		DatalakeContext db,
		int[] identifiers)
	{
		return await db.QueryToArrayAsync<TagHistory>(ReadLast
			.MapIdentifiers(identifiers));
	}

	internal static async Task<TagHistory[]> ProtectedReadLastValuesBeforeDateAsync(
		DatalakeContext db,
		int[] identifiers,
		DateTime date)
	{
		return await db.QueryToArrayAsync<TagHistory>(ReadLastBeforeDate
			.MapIdentifiers(identifiers)
			.MapDate("@old", date));
	}

	private static List<TagHistory> StretchByResolution(
		List<TagHistory> valuesByChange,
		DateTime old,
		DateTime young,
		TagResolution resolution)
	{
		var timeRange = (young - old).TotalMilliseconds;
		var continuous = new List<TagHistory>();
		DateTime stepDate = old;

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

			stepDate = resolution switch
			{
				TagResolution.BySecond => stepDate.AddSeconds(1),
				TagResolution.ByMinute => stepDate.AddMinutes(1),
				TagResolution.ByHalfHour => stepDate.AddMinutes(30),
				TagResolution.ByHour => stepDate.AddHours(1),
				TagResolution.ByDay => stepDate.AddDays(1),
				TagResolution.ByWeek => stepDate.AddDays(7),
				TagResolution.ByMonth => stepDate.AddMonths(1),
				_ => young,
			};
		}
		while (stepDate <= young);

		return continuous;
	}

	/// <summary>
	/// Расчет средневзвешенных и взвешенных сумм по тегам. Взвешивание по секундам
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="identifiers">Идентификаторы тегов</param>
	/// <param name="moment">Момент времени, относительно которого определяется прошедший период</param>
	/// <param name="period">Размер прошедшего периода</param>
	/// <returns>По одному значению на каждый тег</returns>
	/// <exception cref="ForbiddenException"></exception>
	public static async Task<TagAggregationWeightedValue[]> GetWeightedAggregatedValuesAsync(
		DatalakeContext db,
		int[] identifiers,
		DateTime? moment = null,
		AggregationPeriod period = AggregationPeriod.Hour)
	{
		// Задаем входные параметры
		var now = moment ?? DateFormats.GetCurrentDateTime();

		DateTime periodStart, periodEnd;
		switch (period)
		{
			case AggregationPeriod.Munite:
				periodEnd = now.RoundByResolution(TagResolution.ByMinute);
				periodStart = periodEnd.AddMinutes(-1);
				break;
			case AggregationPeriod.Hour:
				periodEnd = now.RoundByResolution(TagResolution.ByHour);
				periodStart = periodEnd.AddHours(-1);
				break;
			case AggregationPeriod.Day:
				periodEnd = now.RoundByResolution(TagResolution.ByDay);
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
				raw.Date >  periodStart &&
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
	}

	#endregion

	#region Запись

	/// <summary>
	/// Запись значений в партицию через временную таблицу
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="records">Список записей для ввода</param>
	internal async Task ProtectedWriteValuesAsync(
		DatalakeContext db,
		List<TagHistory> records)
	{
		if (records.Count == 0)
			return;

		var uniqueRecords = records.GroupBy(x => new { x.TagId, x.Date })
			.Select(g => g.First())
			.ToArray();

		using var transaction = await db.BeginTransactionAsync();

		try
		{
			await db.ExecuteAsync(CreateTempForWrite);
			await db.BulkCopyAsync(bulkCopyOptions, uniqueRecords);
			await db.ExecuteAsync(Write);

			await transaction.CommitAsync();

			// обновление в кэше текущих данных
			foreach (var record in uniqueRecords)
				valuesStore.TryUpdate(record.TagId, record);
		}
		catch (Exception e)
		{
			logger.LogError(e, "Не удалось записать данные");
			await transaction.RollbackAsync();
		}
	}

	#endregion

	#region SQL

	const string StagingTable = "TagsHistoryState";

	static BulkCopyOptions bulkCopyOptions = new() { TableName = StagingTable, BulkCopyType = BulkCopyType.ProviderSpecific, };

	const string CreateTempForWrite = $@"
		CREATE TEMPORARY TABLE ""{StagingTable}"" (LIKE public.""TagsHistory"" EXCLUDING INDEXES)
		ON COMMIT DROP;";

	const string Write = $@"
		INSERT INTO public.""TagsHistory""(
			""TagId"", ""Date"", ""Text"", ""Number"", ""Quality""
		)
		SELECT ""TagId"", ""Date"", ""Text"", ""Number"", ""Quality""
			FROM ""{StagingTable}""
		ON CONFLICT (""TagId"", ""Date"") DO UPDATE
			SET ""Text""   = EXCLUDED.""Text"",
					""Number"" = EXCLUDED.""Number"",
					""Quality""= EXCLUDED.""Quality"";";

	/// <summary>
	/// Параметры: нет
	/// </summary>
	const string ReadAllLast = @"
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
	const string ReadLast = @"
		SELECT DISTINCT ON (""TagId"") *
		FROM public.""TagsHistory""
		WHERE ""TagId"" IN (@tags)
		ORDER BY ""TagId"", ""Date"" DESC;";

	/// <summary>
	/// Параметры: теги, дата начала
	/// </summary>
	const string ReadLastBeforeDate = @"
		SELECT DISTINCT ON (""TagId"") *
		FROM public.""TagsHistory""
		WHERE
			""TagId"" IN (@tags)
			AND ""Date"" <= '@old'
		ORDER BY ""TagId"", ""Date"" DESC";

	/// <summary>
	/// Параметры: теги, дата начала, дата конца
	/// </summary>
	const string ReadBetweenDates = $@"
		SELECT * FROM (
			{ReadLastBeforeDate}
		) AS valuesBefore
		UNION ALL
		SELECT *
		FROM public.""TagsHistory""
		WHERE
			""TagId"" IN (@tags)
			AND ""Date"" > '@old'
			AND ""Date"" <= '@young';";

	#endregion
}
