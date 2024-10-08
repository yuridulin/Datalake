using Datalake.ApiClasses.Constants;
using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Exceptions;
using Datalake.ApiClasses.Models.Tags;
using Datalake.ApiClasses.Models.Values;
using Datalake.Database.Extensions;
using Datalake.Database.Models;
using Datalake.Database.Models.Classes;
using Datalake.Database.Utilities;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Datalake.Database.Repositories;

public class ValuesRepository(DatalakeContext db) : IDisposable
{
	#region Действия

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

	#endregion

	#region Системные действия

	public async Task WriteValuesAsSystemAsync(
		ValueWriteRequest[] requests)
	{
		await WriteValuesAsync(requests, false);
	}

	public async Task<HistoryTableInfo[]> GetHistoryTablesFromSchema()
	{
		return await PostgreSQL_GetHistoryTablesFromSchema();
	}

	public async Task CreateHistoryIndex(string tableName)
	{
		await PostgreSQL_CreateHistoryIndex(tableName);
	}

	#endregion

	#region Реализация

	internal static readonly string NamePrefix = "TagsHistory_";
	internal static readonly string DateMask = "yyyy_MM_dd";
	internal static readonly string IndexPostfix = "_idx";

	static readonly ILogger logger = LogManager.CreateLogger<ValuesRepository>();

	#region Манипулирование таблицами

	internal static DateTime GetTableDate(string tableName) => DateTime.TryParseExact(
		tableName.AsSpan(NamePrefix.Length),
		DateMask,
		null,
		System.Globalization.DateTimeStyles.None,
		out var d) ? d : DateTime.MinValue;

	static string GetTableName(DateTime date) => NamePrefix + date.ToString(DateMask);

	internal ITable<TagHistory> GetHistoryTable(DateTime seekDate)
	{
		DateTime date = seekDate.Date;
		ITable<TagHistory> table;

		if (Cache.Tables.TryGetValue(date, out string? value))
		{
			table = db.GetTable<TagHistory>().TableName(value);
		}
		else
		{
			var tableName = GetTableName(date);
			table = db.CreateTable<TagHistory>(tableName);

			lock (Cache.Tables)
			{
				Cache.Tables.Add(date, tableName);
			}
		}

		return table;
	}

	async Task<HistoryTableInfo[]> PostgreSQL_GetHistoryTablesFromSchema()
	{
		var tables = await db.QueryToArrayAsync<HistoryTableWithIndex>($@"
			SELECT t.table_name AS ""Name"", i.indexname AS ""Index""
				FROM information_schema.TABLES t
				LEFT JOIN pg_indexes i ON i.tablename = t.table_name
				WHERE t.table_schema = 'public'
				AND table_name LIKE '{NamePrefix}_%';");

		return tables
			.Select(x => new HistoryTableInfo
			{
				Name = x.Name,
				Date = GetTableDate(x.Name),
				HasIndex = !string.IsNullOrEmpty(x.Index),
			})
			.ToArray();
	}

	async Task PostgreSQL_CreateHistoryIndex(string tableName)
	{
		await db.ExecuteAsync($"CREATE INDEX {tableName.ToLower()}{IndexPostfix} " +
			$"ON public.\"{tableName}\" (\"{nameof(TagHistory.TagId)}\", \"{nameof(TagHistory.Date)}\" DESC);");
	}

	#endregion


	#region Запись значений

	internal async Task InitializeValueAsync(int tagId)
	{
		var record = new TagHistory
		{
			Date = DateTime.Now,
			Number = null,
			Text = null,
			Quality = TagQuality.Unknown,
			TagId = tagId,
		};

		Live.Write([record]);

		var table = GetHistoryTable(record.Date);
		await table.BulkCopyAsync([record]);
	}

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
				info = Cache.Tags.TryGetValue(writeRequest.Id.Value, out var i)
					? i
					: throw new NotFoundException($"тег [#{writeRequest.Id}]");
			}
			else if (writeRequest.Guid != null)
			{
				info = Cache.Tags.Values
					.Where(x => x.Guid == writeRequest.Guid)
					.FirstOrDefault()
					?? throw new NotFoundException($"тег [{writeRequest.Guid}]");
			}
			else
			{
				continue;
			}

			var record = info.ToHistory(writeRequest.Value, writeRequest.Quality);
			if (!Live.IsNew(record) && !overrided)
				continue;
			record.Date = writeRequest.Date ?? DateTime.Now;

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

		Live.Write(recordsToWrite);
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
			var table = GetHistoryTable(g.Key);
			await table.BulkCopyAsync(records);
		}

		sw.Stop();
		logger.LogInformation("Запись архивных значений: [{n}] за {ms} мс", records.Count(), sw.Elapsed.TotalMilliseconds);
	}

	#endregion


	#region Чтение значений

	internal async Task<List<ValuesResponse>> GetValuesAsync(ValuesRequest[] requests)
	{
		List<ValuesResponse> responses = [];

		// TODO: группировка тегов по диапазонам, чтобы прочитать с минимумом обращений к БД
		// Такой подход выиграет, если много запросов с примерно одинаковым диапазоном времени

		foreach (var request in requests)
		{
			int[] trustedIdentifiers = [
				.. Cache.Tags.Keys.Where(x => request.TagsId?.Contains(x) ?? false),
				.. Cache.Tags.Values.Where(x => request.Tags?.Contains(x.Guid) ?? false).Select(x => x.Id)
			];

			if (trustedIdentifiers.Length == 0)
				continue;

			ValuesResponse response = new()
			{
				RequestKey = request.RequestKey,
				Tags = []
			};

			// Если не указывается ни одна дата, выполняется получение текущих значений. Не убирать!
			if (!request.Exact.HasValue && !request.Old.HasValue && !request.Young.HasValue)
			{
				response.Tags = Live.Read(trustedIdentifiers)
					.Select(x => new { Info = Cache.Tags[x.TagId], Value = x })
					.Select(x => new ValuesTagResponse
					{
						Id = x.Info.Id,
						Guid = x.Info.Guid,
						Name = x.Info.Name,
						Type = x.Info.TagType,
						Values = [ new()
						{
							Date = x.Value.Date,
							DateString = x.Value.Date.ToString(DateFormats.HierarchicalWithMilliseconds),
							Quality = x.Value.Quality,
							Value = x.Value.GetTypedValue(x.Info.TagType),
						}]
					})
					.ToList();
			}
			else
			{
				DateTime exact = request.Exact ?? DateTime.Now;
				DateTime old, young;

				// Получение истории
				if (request.Exact.HasValue)
				{
					young = request.Exact.Value;
					old = request.Exact.Value;
				}
				else
				{
					young = request.Young ?? exact;
					old = request.Old ?? young.Date;
				}

				var databaseValues = await ReadHistoryValuesAsync(
					trustedIdentifiers, old, young, resolution: Math.Max(0, request.Resolution ?? 0));

				// сборка ответа, агрегация по необходимости
				foreach (var id in trustedIdentifiers)
				{
					var tagInfo = Cache.Tags[id];
					var existValues = databaseValues
						.Where(x => x.TagId == id)
						.ToArray();

					if (request.Func == AggregationFunc.List)
					{
						response.Tags.Add(new ValuesTagResponse
						{
							Id = tagInfo.Id,
							Guid = tagInfo.Guid,
							Name = tagInfo.Name,
							Type = tagInfo.TagType,
							Values = [.. existValues
							.Select(x => new ValueRecord
							{
								Date = x.Date,
								DateString = x.Date.ToString(DateFormats.HierarchicalWithMilliseconds),
								Quality = x.Quality,
								Value = x.GetTypedValue(tagInfo.TagType),
							})
							.OrderBy(x => x.Date)],
						});
					}
					else if (tagInfo.TagType == TagType.Number)
					{
						var values = existValues
							.Where(x => x.Quality == TagQuality.Good || x.Quality == TagQuality.Good_ManualWrite)
							.Select(x => x.GetTypedValue(tagInfo.TagType) as float?)
							.ToList();

						if (values.Count > 0)
						{
							float? value = 0;
							try
							{
								switch (request.Func)
								{
									case AggregationFunc.Sum:
										value = values.Sum();
										break;
									case AggregationFunc.Avg:
										value = values.Average();
										break;
									case AggregationFunc.Min:
										value = values.Min();
										break;
									case AggregationFunc.Max:
										value = values.Max();
										break;
								}
							}
							catch
							{
							}

							response.Tags.Add(new ValuesTagResponse
							{
								Id = tagInfo.Id,
								Guid = tagInfo.Guid,
								Name = tagInfo.Name,
								Type = tagInfo.TagType,
								Values = [
										new ValueRecord
									{
										Date = exact,
										DateString = exact.ToString(DateFormats.HierarchicalWithMilliseconds),
										Quality = TagQuality.Good,
										Value = value,
									}
									]
							});
						}
						else
						{
							response.Tags.Add(new ValuesTagResponse
							{
								Id = tagInfo.Id,
								Guid = tagInfo.Guid,
								Name = tagInfo.Name,
								Type = tagInfo.TagType,
								Values = [
										new ValueRecord
									{
										Date = exact,
										DateString = exact.ToString(DateFormats.HierarchicalWithMilliseconds),
										Quality = TagQuality.Bad_NoValues,
										Value = 0,
									}
									]
							});
						}
					}
				}
			}

			responses.Add(response);
		}

		return responses;
	}

	internal async Task<List<TagHistory>> ReadHistoryValuesAsync(
		int[] identifiers,
		DateTime old,
		DateTime young,
		int resolution = 0)
	{
		logger.LogInformation("Событие чтения архивных значений");

		var firstDate = old.Date;
		var lastDate = young.Date;
		var seekDate = lastDate;
		var values = new List<TagHistory>();
		Stopwatch d;

		var tables = Cache.Tables
			.Where(x => x.Key >= firstDate && x.Key <= lastDate)
			.Union(Cache.Tables.Where(x => x.Key <= firstDate).OrderByDescending(x => x.Key).Take(1))
			.ToDictionary();

		d = Stopwatch.StartNew();
		List<IQueryable<TagHistory>> queries = [];

		ITable<TagHistory> table = null!;

		// проход по циклу, чтобы выгрузить все данные между old и young
		do
		{
			if (tables.ContainsKey(seekDate))
			{
				table = db.GetTable<TagHistory>().TableName(GetTableName(seekDate));

				var query = table
					.Where(x => identifiers.Contains(x.TagId));

				if (seekDate == lastDate) query = query.Where(x => x.Date <= young);
				if (seekDate == firstDate) query = query.Where(x => x.Date >= old);

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
				table = db.GetTable<TagHistory>().TableName(GetTableName(lastStoredDate));
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
			.Select(LostTag)
			.ToArray();

		if (lost.Length > 0)
		{
			values.AddRange(lost);
		}

		// выполняем протяжку, если необходимо
		if (resolution > 0)
		{
			d = Stopwatch.StartNew();
			logger.LogInformation("Протяжка данных...");

			var timeRange = (young - old).TotalMilliseconds;
			var continuous = new List<TagHistory>();
			DateTime stepDate;

			for (double i = 0; i < timeRange; i += resolution)
			{
				stepDate = old.AddMilliseconds(i);

				foreach (var id in identifiers)
				{
					var value = values
						.Where(x => x.TagId == id)
						.Where(x => x.Date <= stepDate)
						.OrderByDescending(x => x.Date)
						.FirstOrDefault();

					if (value != null)
					{
						if (value.Date != stepDate)
						{
							continuous.Add(new TagHistory
							{
								TagId = id,
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
				}
			}

			values = continuous;

			d.Stop();
			logger.LogInformation("Протяжка завершена: {ms} мс", d.Elapsed.TotalMilliseconds);
		}

		return values;

		TagHistory LostTag(int id) => new()
		{
			TagId = id,
			Date = old,
			Text = null,
			Number = null,
			Quality = TagQuality.Bad,
		};
	}

	#endregion


	public void Dispose()
	{
		db.Close();
		GC.SuppressFinalize(this);
	}

	#endregion
}
