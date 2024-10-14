using Datalake.ApiClasses.Constants;
using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Exceptions;
using Datalake.ApiClasses.Models.Tags;
using Datalake.ApiClasses.Models.Values;
using Datalake.Database.Extensions;
using Datalake.Database.Models;
using Datalake.Database.Utilities;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Datalake.Database.Repositories;

public class ValuesRepository(DatalakeContext db)
{
	public static Dictionary<int, TagHistory> LiveValues { get; set; } = [];

	#region Действия

	public static List<TagHistory> GetLiveValues(int[] identifiers)
	{
		return identifiers
			.Select(id => LiveValues.TryGetValue(id, out var value) ? value : LostTag(id, DateFormats.GetCurrentDateTime()))
			.ToList();
	}

	public async Task CreateLiveValues()
	{
		var tags = db.Tags.Select(x => x.Id).ToArray();
		var lastValues = await ReadHistoryValuesAsync(tags, DateFormats.GetCurrentDateTime(), DateFormats.GetCurrentDateTime());

		await db.Logs.BulkCopyAsync(lastValues.Select(v => new Log
		{
			Category = LogCategory.Database,
			Date = v.Date,
			RefId = v.TagId.ToString(),
			Type = LogType.Trace,
			Text = $"Значение тега для кэша: {v.Number} | {v.Text} | {v.Quality}",
		}));

		lock (locker)
		{
			LiveValues = lastValues
				.ToDictionary(x => x.TagId, x => x);
		}
	}

	public static void WriteLiveValues(IEnumerable<TagHistory> values)
	{
		lock (locker)
		{
			foreach (var value in values)
			{
				if (!LiveValues.TryGetValue(value.TagId, out TagHistory? exist))
				{
					LiveValues.Add(value.TagId, value);
				}
				else if (exist.Date <= value.Date)
				{
					LiveValues[exist.TagId] = value;
				}
			}
		}
	}

	public static bool IsValueNew(TagHistory value)
	{
		if (LiveValues.TryGetValue(value.TagId, out var old))
		{
			return !old.Equals(value);
		}
		else
		{
			return true;
		}
	}

	public async Task<List<ValuesResponse>> GetValuesAsync(
		ValuesRequest[] requests,
		Guid? energoId = null)
	{
		// TODO: energoId
		if (energoId.HasValue)
		{ }

		return await GetValuesAsync(requests);
	}

	public async Task<List<ValuesTagResponse>> WriteValuesAsync(
		ValueWriteRequest[] requests,
		bool overrided = false,
		Guid? energoId = null)
	{
		// TODO: energoId
		if (energoId.HasValue)
		{ }

		return await WriteValuesAsync(requests, overrided);
	}

	public async Task WriteValuesAsSystemAsync(
		ValueWriteRequest[] requests)
	{
		await WriteValuesAsync(requests, false);
	}

	#endregion



	static readonly ILogger logger = LogManager.CreateLogger<ValuesRepository>();

	static object locker = new();

	#region Запись значений

	/// <summary>
	/// Запись новых значений, с обновлением текущих при необходимости
	/// </summary>
	/// <param name="requests">Список запросов на запись</param>
	/// <returns>Ответ со списком значений, как при чтении</returns>
	/// <exception cref="NotFoundException">Тег не найден</exception>
	internal async Task<List<ValuesTagResponse>> WriteValuesAsync(ValueWriteRequest[] requests, bool overrided = false)
	{
		List<ValuesTagResponse> responses = [];
		List<TagHistory> recordsToWrite = [];

		foreach (var writeRequest in requests)
		{
			TagCacheInfo? info = null;

			if (writeRequest.Id != null)
			{
				info = TagsRepository.CachedTags.TryGetValue(writeRequest.Id.Value, out var i)
					? i
					: throw new NotFoundException($"тег [#{writeRequest.Id}]");
			}
			else if (writeRequest.Guid != null)
			{
				info = TagsRepository.CachedTags.Values
					.Where(x => x.Guid == writeRequest.Guid)
					.FirstOrDefault()
					?? throw new NotFoundException($"тег [{writeRequest.Guid}]");
			}
			else
			{
				continue;
			}

			var record = info.ToHistory(writeRequest.Value, writeRequest.Quality);
			if (!IsValueNew(record) && !overrided)
				continue;
			record.Date = writeRequest.Date ?? DateFormats.GetCurrentDateTime();

			recordsToWrite.Add(record);

			responses.Add(new ValuesTagResponse
			{
				Id = info.Id,
				Guid = info.Guid,
				Name = info.Name,
				Type = info.TagType,
				Values = [
					new ValueRecord
					{
						Date = record.Date,
						DateString = record.Date.ToString(DateFormats.HierarchicalWithMilliseconds),
						Quality = record.Quality,
						Value = record.GetTypedValue(info.TagType),
					}
				]
			});
		}

		WriteLiveValues(recordsToWrite);
		await WriteHistoryValuesAsync(recordsToWrite);

		return responses;
	}

	async Task WriteHistoryValuesAsync(IEnumerable<TagHistory> records)
	{
		var sw = Stopwatch.StartNew();
		logger.LogInformation("Событие записи архивных значений");

		if (!records.Any())
			return;

		foreach (var g in records.GroupBy(x => x.Date.Date))
		{
			var table = db.TablesRepository.GetHistoryTable(g.Key);
			var values = g.Select(x => x);
			await table.BulkCopyAsync(values);

			// Если пишем в прошлое, нужно обновить стартовые записи в будущем
			if (g.Key < DateTime.Today)
			{
				await UpdateInitialValuesInFuture(db, records, g);
			}
		}

		sw.Stop();
		logger.LogInformation("Запись архивных значений: [{n}] за {ms} мс", records.Count(), sw.Elapsed.TotalMilliseconds);
	}

	private static async Task UpdateInitialValuesInFuture(
		DatalakeContext db,
		IEnumerable<TagHistory> records,
		IGrouping<DateTime, TagHistory> g)
	{
		var sw = Stopwatch.StartNew();
		logger.LogInformation("Обновление initial значений в будущих таблицах");

		DateTime date = g.Key;

		do
		{
			var nextDate = TablesRepository.GetNextTable(date);
			if (!nextDate.HasValue)
				break;

			var tagsWithoutNextValues = await db.TablesRepository.GetHistoryTable(date)
				.Where(x => !records.Any(r => r.TagId == x.TagId && r.Date < x.Date))
				.Select(x => x.TagId)
				.ToArrayAsync();

			if (tagsWithoutNextValues.Length == 0)
				break;

			var nextTable = db.TablesRepository.GetHistoryTable(nextDate.Value);

			var tagsWithNextInitialValues = await nextTable
				.Where(x => tagsWithoutNextValues.Contains(x.TagId))
				.Where(x => x.Quality == TagQuality.Bad_LOCF || x.Quality == TagQuality.Good_LOCF)
				.Select(x => x.TagId)
				.ToArrayAsync();

			await nextTable
				.Join(records.Where(x => tagsWithNextInitialValues.Contains(x.TagId)),
					data => data.TagId, updated => updated.TagId, (data, updated) => new { data, updated })
				.Set(joined => joined.data.Text, joined => joined.updated.Text)
				.Set(joined => joined.data.Number, joined => joined.updated.Number)
				.Set(joined => joined.data.Quality, joined => joined.updated.Quality == TagQuality.Bad_ManualWrite
					? TagQuality.Bad_LOCF
					: TagQuality.Good_LOCF)
				.UpdateAsync();

			await nextTable
				.BulkCopyAsync(records
					.Where(x => tagsWithoutNextValues.Contains(x.TagId) && !tagsWithNextInitialValues.Contains(x.TagId))
					.Select(x => new TagHistory
					{
						TagId = x.TagId,
						Date = nextDate.Value,
						Number = x.Number,
						Text = x.Text,
						Quality = x.Quality == TagQuality.Bad_ManualWrite ? TagQuality.Bad_LOCF : TagQuality.Good_LOCF
					}));

			date = nextDate.Value;
		}
		while (true);

		sw.Stop();
		logger.LogInformation("Обновление initial значений в будущих таблицах: [{n}] за {ms} мс", records.Count(), sw.Elapsed.TotalMilliseconds);
	}

	#endregion


	#region Чтение значений

	internal async Task<List<ValuesResponse>> GetValuesAsync(ValuesRequest[] requests)
	{
		List<ValuesResponse> responses = [];

		var trustedRequests = requests.Select(x => new
		{
			x.RequestKey,
			Time = new
			{
				x.Old,
				x.Young,
				x.Exact,
			},
			x.Resolution,
			x.Func,
			Tags = TagsRepository.CachedTags.Values.Where(t => x.TagsId != null && x.TagsId.Contains(t.Id))
				.Union(TagsRepository.CachedTags.Values.Where(t => x.Tags != null && x.Tags.Contains(t.Guid)))
				.ToArray(),
		});

		foreach (var timeGroup in trustedRequests.GroupBy(x => x.Time))
		{
			var timeSettings = timeGroup.Key;
			var processedRequests = timeGroup.ToArray();
			var trustedIdentifiers = processedRequests.SelectMany(x => x.Tags.Select(t => t.Id)).ToArray();

			// Если не указывается ни одна дата, выполняется получение текущих значений. Не убирать!
			if (!timeSettings.Exact.HasValue && !timeSettings.Old.HasValue && !timeSettings.Young.HasValue)
			{
				var values = GetLiveValues(trustedIdentifiers);

				foreach (var request in processedRequests)
				{
					var response = new ValuesResponse
					{
						RequestKey = request.RequestKey,
						Tags = (
							from value in values
							join tag in request.Tags on value.TagId equals tag.Id
							select new ValuesTagResponse
							{
								Id = tag.Id,
								Guid = tag.Guid,
								Name = tag.Name,
								Type = tag.TagType,
								Values = [ new()
								{
									Date = value.Date,
									DateString = value.Date.ToString(DateFormats.HierarchicalWithMilliseconds),
									Quality = value.Quality,
									Value = value.GetTypedValue(tag.TagType),
								}]
							}
						).ToList(),
					};

					responses.Add(response);
				}
			}
			else
			{
				DateTime exact = timeSettings.Exact ?? DateFormats.GetCurrentDateTime();
				DateTime old, young;

				// Получение истории
				if (timeSettings.Exact.HasValue)
				{
					young = timeSettings.Exact.Value;
					old = timeSettings.Exact.Value;
				}
				else
				{
					young = timeSettings.Young ?? exact;
					old = timeSettings.Old ?? young.Date;
				}

				var databaseValues = await ReadHistoryValuesAsync(trustedIdentifiers, old, young);

				foreach (var request in processedRequests)
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
							Type = tag.TagType,
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

							if (tag.TagType == TagType.Number && request.Func != AggregationFunc.List)
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
								catch (Exception e)
								{
									logger.LogError("Ошибка при агрегировании: {message}", e.Message);
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
										Value = x.GetTypedValue(tag.TagType),
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

	internal async Task<List<TagHistory>> ReadHistoryValuesAsync(
		int[] identifiers,
		DateTime old,
		DateTime young)
	{
		logger.LogInformation("Событие чтения архивных значений");

		var firstDate = old.Date;
		var lastDate = young.Date;
		var seekDate = lastDate;
		var values = new List<TagHistory>();
		Stopwatch d;

		var tables = TablesRepository.CachedTables
			.Where(x => x.Key >= firstDate && x.Key <= lastDate)
			.Union(TablesRepository.CachedTables.Where(x => x.Key <= firstDate).OrderByDescending(x => x.Key).Take(1))
			.ToDictionary();

		d = Stopwatch.StartNew();
		List<IQueryable<TagHistory>> queries = [];

		ITable<TagHistory> table = null!;

		// проход по циклу, чтобы выгрузить все данные между old и young
		do
		{
			if (tables.ContainsKey(seekDate))
			{
				table = db.GetTable<TagHistory>().TableName(TablesRepository.GetTableName(seekDate));

				var query = table
					.Where(x => identifiers.Contains(x.TagId));

				if (seekDate == lastDate)
					query = query.Where(x => x.Date <= young);
				if (seekDate == firstDate)
					query = query.Where(x => x.Date >= old);

				queries.Add(query);
			}

			seekDate = seekDate.AddDays(-1);
		}
		while (seekDate >= firstDate);

		// CTE для поиска последних по дате (перед old) значений для каждого из тегов
		if (table == null)
		{
			var lastStoredDate = tables.Keys
				.Where(x => x <= lastDate)
				.OrderByDescending(x => x)
				.DefaultIfEmpty(DateTime.MinValue)
				.FirstOrDefault();

			if (lastStoredDate != DateTime.MinValue)
			{
				table = db.GetTable<TagHistory>().TableName(TablesRepository.GetTableName(lastStoredDate));
			}
		}

		if (table != null)
		{
			var tagsBeforeOld =
				from th in table
				where identifiers.Contains(th.TagId)
					&& th.Date < old
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
				};

			queries.Add(
				from rt in tagsBeforeOld
				where rt.rn == 1
				select new TagHistory
				{
					TagId = rt.TagId,
					Date = old,
					Text = rt.Text,
					Number = rt.Number,
					Quality = rt.Quality,
				});
		}

		// мегазапрос для получения всех необходимых данных
		if (queries.Count > 0)
		{
			var megaQuery = queries.Aggregate((current, next) => current.Union(next));

			// выполнение мегазапроса
			values = await megaQuery.ToListAsync();
		}

		d.Stop();
		logger.LogInformation("Чтение значений из БД: {ms} мс", d.Elapsed.TotalMilliseconds);


		// заглушки, если значения так и не были проинициализированы
		d = Stopwatch.StartNew();
		var lost = identifiers
			.Except(values.Where(x => x.Date == old).Select(x => x.TagId))
			.Select(id => LostTag(id, old))
			.ToArray();

		if (lost.Length > 0)
		{
			values.AddRange(lost);
		}

		return values;
	}

	static List<TagHistory> StretchByResolution(
		IEnumerable<TagHistory> valuesByChange,
		DateTime old,
		DateTime young,
		int resolution)
	{
		var d = Stopwatch.StartNew();
		logger.LogInformation("Протяжка данных...");

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
						Quality = value.Quality,
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

		d.Stop();
		logger.LogInformation("Протяжка завершена: {ms} мс", d.Elapsed.TotalMilliseconds);

		return continuous;
	}

	static TagHistory LostTag(int id, DateTime date) => new()
	{
		TagId = id,
		Date = date,
		Text = null,
		Number = null,
		Quality = TagQuality.Bad,
	};

	#endregion
}
