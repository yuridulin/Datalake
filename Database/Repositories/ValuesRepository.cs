using Datalake.Database.Constants;
using Datalake.Database.Enums;
using Datalake.Database.Exceptions;
using Datalake.Database.Extensions;
using Datalake.Database.Models.Auth;
using Datalake.Database.Models.Tags;
using Datalake.Database.Models.Values;
using Datalake.Database.Tables;
using LinqToDB;
using LinqToDB.Data;
using System.Diagnostics;

namespace Datalake.Database.Repositories;

/// <summary>
/// Репозиторий для работы со значениями тегов
/// </summary>
public class ValuesRepository(DatalakeContext db)
{
	#region Действия

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
		var trustedRequests = requests.Select(x => new ValuesTrustedRequest
		{
			RequestKey = x.RequestKey,
			Time = new ValuesTrustedRequest.TimeSettings
			{
				Old = x.Old,
				Young = x.Young,
				Exact = x.Exact,
			},
			Resolution = x.Resolution,
			Func = x.Func,
			Tags = TagsRepository.CachedTags.Values.Where(t => x.TagsId != null && x.TagsId.Contains(t.Id))
				.Union(TagsRepository.CachedTags.Values.Where(t => x.Tags != null && x.Tags.Contains(t.Guid)))
				.Where(x => AccessRepository.HasAccessToTag(user, AccessType.Viewer, x.Guid))
				.ToArray(),
		});

		return await GetValuesAsync(trustedRequests);
	}

	/// <summary>
	/// Запись новых значений для указанных тегов
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="requests">Список тегов с новыми значениями</param>
	/// <param name="overrided">Нужно ли выполнять запись, если нет изменений с текущим значением</param>
	/// <returns>Список записанных значений</returns>
	public async Task<List<ValuesTagResponse>> WriteValuesAsync(
		UserAuthInfo user,
		ValueWriteRequest[] requests,
		bool overrided = false)
	{
		var trustedRequests = new List<ValueTrustedWriteRequest>();

		foreach (var request in requests)
		{
			TagCacheInfo? tag = null;
			if (request.Id.HasValue && TagsRepository.CachedTags.TryGetValue(request.Id.Value, out var t))
			{
				tag = t;
			}
			else if (request.Guid.HasValue)
			{
				tag = TagsRepository.CachedTags.Values
					.Where(x => x.Guid == request.Guid)
					.FirstOrDefault();
			}

			if (tag != null && AccessRepository.HasAccessToTag(user, AccessType.Editor, tag.Guid))
			{
				trustedRequests.Add(new()
				{
					Tag = tag,
					Date = request.Date,
					Quality = request.Quality,
					Value = request.Value
				});
			}
		}

		return await WriteValuesAsync(trustedRequests, overrided);
	}

	/// <summary>
	/// Запись значений на уровне приложения, без проверок на уровень доступа.
	/// Используется для сборщиков
	/// </summary>
	/// <param name="requests">Список записанных значений</param>
	/// <returns>Затраченное на запись время в миллисекундах</returns>
	public async Task<int> WriteValuesAsSystemAsync(
		ValueWriteRequest[] requests)
	{
		var stopwatch = Stopwatch.StartNew();

		var recordsToWrite = new List<TagHistory>();

		foreach (var request in requests)
		{
			TagCacheInfo? tag = null;
			if (request.Id.HasValue && TagsRepository.CachedTags.TryGetValue(request.Id.Value, out var t))
			{
				tag = t;
			}
			else if (request.Guid.HasValue)
			{
				tag = TagsRepository.CachedTags.Values
					.Where(x => x.Guid == request.Guid)
					.FirstOrDefault();
			}

			if (tag != null)
			{
				var record = tag.ToHistory(request.Value, request.Quality);
				if (!IsValueNew(record))
					continue;
				record.Date = request.Date ?? DateFormats.GetCurrentDateTime();

				recordsToWrite.Add(record);
			}
		}

		WriteLiveValues(recordsToWrite);
		await WriteNewValuesAsync(recordsToWrite);

		stopwatch.Stop();
		return Convert.ToInt32(stopwatch.Elapsed.TotalMilliseconds);
	}

	#endregion

	#region Кэш

	/// <summary>
	/// Кэшированные текущие значения тегов, сопоставленные с идентификаторами
	/// </summary>
	internal static Dictionary<int, TagHistory> LiveValues { get; set; } = [];

	internal static List<TagHistory> GetLiveValues(int[] identifiers)
	{
		return identifiers
			.Select(id => LiveValues.TryGetValue(id, out var value) ? value : LostTag(id, DateFormats.GetCurrentDateTime()))
			.ToList();
	}

	internal async Task CreateLiveValues()
	{
		var tags = db.Tags.Select(x => x.Id).ToArray();
		var date = DateFormats.GetCurrentDateTime();

		var table = db.TablesRepository.GetHistoryTable(DateFormats.GetCurrentDateTime().Date);
		var count = await table.CountAsync();

		if (count == 0)
		{
			var lastDate = TablesRepository.GetPreviousTableDate(DateFormats.GetCurrentDateTime().Date);
			if (lastDate != null)
				table = db.TablesRepository.GetHistoryTable(lastDate.Value);
			else
			{
				lock (locker)
				{
					LiveValues = tags.ToDictionary(x => x, x => new TagHistory
					{
						TagId = x,
						Date = DateTime.MinValue,
						Number = null,
						Text = null,
						Quality = TagQuality.Bad,
					});
				}

				return;
			}
		}

		var query =
			from rt in
				from th in table
				where tags.Contains(th.TagId)
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

		var values = await query.ToListAsync();

		lock (locker)
		{
			LiveValues = values
				.DistinctBy(x => x.TagId)
				.ToDictionary(x => x.TagId, x => x);
		}
	}

	internal static void WriteLiveValues(IEnumerable<TagHistory> values)
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

	internal static bool IsValueNew(TagHistory value)
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

	static object locker = new();

	#endregion

	#region Запись значений

	/// <summary>
	/// Запись новых значений, с обновлением текущих при необходимости
	/// </summary>
	/// <param name="requests">Список запросов на запись</param>
	/// <param name="overrided">Обязательная запись, даже если изменений не было</param>
	/// <returns>Ответ со списком значений, как при чтении</returns>
	/// <exception cref="NotFoundException">Тег не найден</exception>
	private async Task<List<ValuesTagResponse>> WriteValuesAsync(
		IEnumerable<ValueTrustedWriteRequest> requests,
		bool overrided = false)
	{
		List<ValuesTagResponse> responses = [];
		List<TagHistory> recordsToWrite = [];

		foreach (var request in requests)
		{
			var record = request.Tag.ToHistory(request.Value, request.Quality);
			if (!IsValueNew(record) && !overrided)
				continue;
			record.Date = request.Date ?? DateFormats.GetCurrentDateTime();

			recordsToWrite.Add(record);

			responses.Add(new ValuesTagResponse
			{
				Id = request.Tag.Id,
				Guid = request.Tag.Guid,
				Name = request.Tag.Name,
				Type = request.Tag.TagType,
				Values = [
					new ValueRecord
					{
						Date = record.Date,
						DateString = record.Date.ToString(DateFormats.HierarchicalWithMilliseconds),
						Quality = record.Quality,
						Value = record.GetTypedValue(request.Tag.TagType),
					}
				]
			});
		}

		WriteLiveValues(recordsToWrite);
		await WriteHistoryValuesAsync(recordsToWrite);

		return responses;
	}

	private async Task WriteHistoryValuesAsync(IEnumerable<TagHistory> records)
	{
		if (!records.Any())
			return;

		foreach (var g in records.GroupBy(x => x.Date.Date))
		{
			var table = db.TablesRepository.GetHistoryTable(g.Key);
			var values = g.Select(x => x);

			string tempTableName = "TagsHistoryInserting_" + DateTime.UtcNow.ToFileTimeUtc().ToString();
			var tempTable = await db.CreateTempTableAsync<TagHistory>(tempTableName);

			await tempTable.BulkCopyAsync(values);

			var existValues =
				from exist in table
				from insert in tempTable.LeftJoin(x => x.Date == exist.Date && x.TagId == exist.TagId)
				where insert != null
				select exist;

			await existValues.DeleteAsync();

			await table.BulkCopyAsync(tempTable);

			await db.DropTableAsync<TagHistory>(tempTableName);

			// Если пишем в прошлое, нужно обновить стартовые записи в будущем
			if (g.Key < DateTime.Today)
			{
				await UpdateInitialValuesInFuture(db, records, g);
			}
		}
	}

	private async Task WriteNewValuesAsync(IEnumerable<TagHistory> records)
	{
		if (!records.Any())
			return;

		foreach (var g in records.GroupBy(x => x.Date.Date))
		{
			var table = db.TablesRepository.GetHistoryTable(g.Key);
			await table.BulkCopyAsync(g.Select(x => x));
		}
	}

	private static async Task UpdateInitialValuesInFuture(
		DatalakeContext db,
		IEnumerable<TagHistory> records,
		IGrouping<DateTime, TagHistory> g)
	{
		DateTime date = g.Key;

		do
		{
			var nextDate = TablesRepository.GetNextTableDate(date);
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
	}

	#endregion

	#region Чтение значений

	internal async Task<List<ValuesResponse>> GetValuesAsync(IEnumerable<ValuesTrustedRequest> trustedRequests)
	{
		List<ValuesResponse> responses = [];

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
			var megaQuery = queries.Aggregate((current, next) => current.UnionAll(next));

			// выполнение мегазапроса
			values = await megaQuery.ToListAsync();
		}

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
