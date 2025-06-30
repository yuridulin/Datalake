using Datalake.Database.Extensions;
using Datalake.Database.Functions;
using Datalake.Database.InMemory;
using Datalake.Database.Models;
using Datalake.Database.Services;
using Datalake.Database.Tables;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Metrics;
using Datalake.PublicApi.Models.Tags;
using Datalake.PublicApi.Models.Values;
using LinqToDB;
using LinqToDB.Data;
using System.Diagnostics;

namespace Datalake.Database.Repositories;

/// <summary>
/// Репозиторий для работы со значениями тегов
/// </summary>
public class ValuesRepository(DatalakeDataStore dataStore, DatalakeCurrentValuesStore valuesStore)
{
	#region Действия

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

		return await ProtectedGetValuesAsync(db, trustedRequests);
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
			if (!valuesStore.TryUpdate(record.TagId, record))
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

			if (tag == null || tag.SourceType != SourceType.Manual || AccessChecks.HasAccessToTag(user, AccessType.Editor, tag.Id))
				continue;

			var record = TagHistoryExtension.CreateFrom(tag, request);
			record.Date = request.Date ?? DateFormats.GetCurrentDateTime();

			recordsToWrite.Add(record);
			valuesStore.TryUpdate(record.TagId, record);

			responses.Add(new ValuesTagResponse
			{
				Id = tag.Id,
				Guid = tag.Guid,
				Name = tag.Name,
				Type = tag.Type,
				Frequency = tag.Frequency,
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
			// Если не указывается ни одна дата, выполняется получение текущих значений. Не убирать!
			if (!group.Settings.Exact.HasValue && !group.Settings.Old.HasValue && !group.Settings.Young.HasValue)
			{
				//var lastValues = await ProtectedReadLastValuesAsync(db, group.TagsId);
				var lastValues = group.TagsId.ToDictionary(x => x, valuesStore.Get);

				foreach (var request in group.Requests)
				{
					var response = new ValuesResponse
					{
						RequestKey = request.RequestKey,
						Tags = request.Tags
							.Select(tag => new ValuesTagResponse
							{
								Id = tag.Id,
								Guid = tag.Guid,
								Name = tag.Name,
								Type = tag.Type,
								Frequency = tag.Frequency,
								SourceType = tag.SourceType,
								Values = !lastValues.TryGetValue(tag.Id, out var value) || value == null ? [] : [
									new()
									{
										Date = value.Date,
										DateString = value.Date.ToString(DateFormats.HierarchicalWithMilliseconds),
										Quality = value.Quality,
										Value = value.GetTypedValue(tag.Type),
									}
								],
							})
							.ToList(),
					};

					responses.Add(response);
				}
			}
			else
			{
				DateTime exact = group.Settings.Exact ?? DateFormats.GetCurrentDateTime();
				DateTime old, young;

				if (group.Settings.Exact.HasValue)
				{
					young = group.Settings.Exact.Value;
					old = group.Settings.Exact.Value;
				}
				else
				{
					young = group.Settings.Young ?? exact;
					old = group.Settings.Old ?? young.Date;
				}

				var (databaseValues, metric) = await ProtectedReadValuesAsync(db, group.TagsId, old, young);

				metric.RequestKeys = group.Requests.Select(x => x.RequestKey).ToArray();
				MetricsService.AddMetric(metric);

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
							Frequency = tag.Frequency,
							SourceType = tag.SourceType,
							Values = [],
						};
						var tagValues = requestValues.Where(x => x.TagId == tag.Id).ToList();

						if (tagValues.Count == 0)
						{
							tagResponse.Values = [
								new()
								{
									Date = exact,
									DateString = exact.ToString(DateFormats.HierarchicalWithMilliseconds),
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
											Date = exact,
											DateString = exact.ToString(DateFormats.HierarchicalWithMilliseconds),
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

	internal static async Task<(List<TagHistory>, HistoryReadMetric)> ProtectedReadValuesAsync(
		DatalakeContext db,
		int[] identifiers,
		DateTime old,
		DateTime young)
	{
		var time = Stopwatch.StartNew();

		var historySet = db.TagsHistory.Where(tag => identifiers.Contains(tag.TagId));

		var historyBeforeOld =
			from rt in
				from th in historySet
				where th.Date <= old
				select new
				{
					th.TagId,
					th.Date,
					th.Text,
					th.Number,
					th.Quality,
					rn = Sql.Ext
						.RowNumber().Over()
						.PartitionBy(th.TagId)
						.OrderByDesc(th.Date)
						.ToValue()
				}
			where rt.rn == 1
			select new TagHistory
			{
				TagId = rt.TagId,
				Date = old,
				Text = rt.Text,
				Number = rt.Number,
				Quality = rt.Quality,
			};

		var historyBetweenRange = historySet
			.Where(tag => tag.Date > old && tag.Date <= young);

		var historyAll = historyBeforeOld
			.Concat(historyBetweenRange)
			.OrderBy(x => x.TagId)
			.ThenBy(x => x.Date);

		var values = await historyAll.ToListAsync();

		time.Stop();

		var metric = new HistoryReadMetric
		{
			Date = DateFormats.GetCurrentDateTime(),
			Elapsed = time.Elapsed,
			TagsId = identifiers,
			Old = old,
			Young = young,
			RecordsCount = values.Count,
			Sql = historyAll.ToString() ?? string.Empty,
		};

		return (values, metric);
	}

	internal static async Task<Dictionary<int, TagHistory>> ProtectedReadLastValuesAsync(
		DatalakeContext db,
		int[]? identifiers = null)
	{
		var time = Stopwatch.StartNew();

		var historySet = db.TagsHistory.Where(tag => identifiers == null || identifiers.Contains(tag.TagId));

		var historyLast =
			from rt in
				from th in historySet
				select new
				{
					th.TagId,
					th.Date,
					th.Text,
					th.Number,
					th.Quality,
					rn = Sql.Ext
						.RowNumber().Over()
						.PartitionBy(th.TagId)
						.OrderByDesc(th.Date)
						.ToValue()
				}
			where rt.rn == 1
			select new TagHistory
			{
				TagId = rt.TagId,
				Date = rt.Date,
				Text = rt.Text,
				Number = rt.Number,
				Quality = rt.Quality,
			};

		var values = await historyLast.ToDictionaryAsync(x => x.TagId);

		time.Stop();

		return values;
	}

	private static List<TagHistory> StretchByResolution(
		List<TagHistory> valuesByChange,
		DateTime old,
		DateTime young,
		int resolution)
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

			stepDate = stepDate.AddMilliseconds(resolution);
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
				periodEnd = now.RoundToFrequency(TagFrequency.ByMinute);
				periodStart = periodEnd.AddMinutes(-1);
				break;
			case AggregationPeriod.Hour:
				periodEnd = now.RoundToFrequency(TagFrequency.ByHour);
				periodStart = periodEnd.AddHours(-1);
				break;
			case AggregationPeriod.Day:
				periodEnd = now.RoundToFrequency(TagFrequency.ByDay);
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
	internal static async Task ProtectedWriteValuesAsync(
		DatalakeContext db,
		List<TagHistory> records)
	{
		if (records.Count == 0)
			return;

		await db.TagsHistory.BulkCopyAsync(records);
	}

	#endregion
}
