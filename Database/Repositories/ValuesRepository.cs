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

public class ValuesRepository(DatalakeContext db) : IDisposable
{
	public static readonly string NamePrefix = "TagsHistory_";
	public static readonly string DateMask = "yyyy_MM_dd";

	static readonly ILogger logger = LogManager.CreateLogger<ValuesRepository>();

	#region Манипулирование таблицами

	public static DateTime GetTableDate(string tableName) => DateTime.TryParseExact(
		tableName.AsSpan(NamePrefix.Length),
		DateMask,
		null,
		System.Globalization.DateTimeStyles.None,
		out var d) ? d : DateTime.MinValue;

	static string GetTableName(DateTime date) => NamePrefix + date.ToString(DateMask);

	async Task<ITable<TagHistory>> GetHistoryTableAsync(DateTime seekDate)
	{
		ITable<TagHistory> table;

		if (Cache.Tables.TryGetValue(seekDate.Date, out string? value))
		{
			table = db.GetTable<TagHistory>().TableName(value);
		}
		else
		{
			table = await CreateHistoryTableAsync(seekDate.Date);
		}

		return table;
	}

	async Task<ITable<TagHistory>> CreateHistoryTableAsync(DateTime date)
	{
		var sw = Stopwatch.StartNew();

		// создание новой таблицы в случае, если её не было
		var tableName = GetTableName(date);
		var dateTime = date.Date;

		logger.LogInformation("Событие создания новой таблицы: {name}", tableName);

		ITable<TagHistory>? newTable = null;
		try
		{
			newTable = db.CreateTable<TagHistory>(tableName);
		}
		catch (Exception ex)
		{
			logger.LogError("Новая таблица не создана: {message}", ex.Message);
			throw new DatabaseException("Новая таблица не создана", ex);
		}

		ITable<TagHistory>? previousTable = null;
		try
		{
			string? previousTableName = Cache.LastTable(dateTime);
			if (previousTableName != null)
				previousTable = db.GetTable<TagHistory>().TableName(previousTableName);
		}
		catch (Exception ex)
		{
			logger.LogError("Предыдущая таблица не получена: {message}", ex.Message);
		}

		// инициализация значений по последним из предыдущей таблицы)
		if (previousTable != null)
		{
			var initialValues = new List<TagHistory>();

			var latestHistoryValues = previousTable
				.Where(x => previousTable
					.GroupBy(x => x.TagId)
					.Select(g => new { Id = g.Key, Date = g.Select(v => v.Date).Max() })
					.Contains(new { Id = x.TagId, x.Date }))
				.Select(x => new TagHistory
				{
					TagId = x.TagId,
					Date = x.Date,
					Number = x.Number,
					Text = x.Text,
					Quality = x.Quality,
					Using = TagUsing.Initial,
				});

			await newTable.BulkCopyAsync(latestHistoryValues);

			var uninitializedValues =
				from t in db.Tags
				from v in previousTable.LeftJoin(x => x.TagId == t.Id)
				where v == null
				select new TagHistory
				{
					TagId = t.Id,
					Date = dateTime,
					Number = null,
					Text = null,
					Quality = TagQuality.Bad_NoValues,
					Using = TagUsing.Initial,
				};

			await newTable.BulkCopyAsync(uninitializedValues);
		}

		lock (Cache.Tables)
		{
			Cache.Tables.Add(dateTime, tableName);
		}

		sw.Stop();
		logger.LogInformation("Новая таблица создана: [{name}] за {ms} мс", tableName, sw.Elapsed.TotalMilliseconds);

		return newTable;
	}

	#endregion


	#region Запись значений

	public async Task InitializeValueAsync(int tagId)
	{
		var record = new TagHistory
		{
			Date = DateTime.Now,
			Number = null,
			Text = null,
			Quality = TagQuality.Unknown,
			TagId = tagId,
			Using = TagUsing.Initial,
		};

		Live.Write([record]);

		var table = await GetHistoryTableAsync(record.Date);
		await table.BulkCopyAsync([record]);
	}

	/// <summary>
	/// Запись новых значений, с обновлением текущих при необходимости
	/// </summary>
	/// <param name="requests">Список запросов на запись</param>
	/// <returns>Ответ со списком значений, как при чтении</returns>
	/// <exception cref="NotFoundException">Тег не найден</exception>
	public async Task<List<ValuesTagResponse>> WriteValuesAsync(ValueWriteRequest[] requests, bool overrided = false)
	{
		List<ValuesTagResponse> responses = [];
		List<TagHistory> recordsToWrite = [];
		List<TagHistory> recordsToSimpleWrite = [];
		List<TagHistory> recordsToManualWrite = [];

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
			record.Using = TagUsing.Basic;

			recordsToWrite.Add(record);

			if (info.SourceType == SourceType.Custom)
			{
				recordsToManualWrite.Add(record);
			}
			else
			{
				recordsToSimpleWrite.Add(record);
			}

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
						Using = record.Using,
					}
				]
			});
		}

		Live.Write(recordsToWrite);
		await WriteHistoryValuesAsync(recordsToSimpleWrite);
		foreach (var record in recordsToManualWrite)
		{
			await WriteManualHistoryValueAsync(record);
		}

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
			var table = await GetHistoryTableAsync(g.Key);
			await table.BulkCopyAsync(records);
		}

		sw.Stop();
		logger.LogInformation("Запись архивных значений: [{n}] за {ms} мс", records.Count(), sw.Elapsed.TotalMilliseconds);
	}

	/// <summary>
	/// Вставка значения с рекурсивным обновлением предшествующих using
	/// </summary>
	/// <param name="record"></param>
	/// <remarks>
	/// Не будет вызываться при записи из источников, так что не особо бьет по производительности.
	/// <br />Тем не менее составлен неоптимально, так как на каждую запись будет выполняться проход по таблицам.
	/// <br />Вариантом лучше видится отдельная таблица Initial значений
	/// </remarks>
	async Task WriteManualHistoryValueAsync(TagHistory record)
	{
		/*
		 * Реализация:
		 * 1. Определить, существует ли таблица на дату записи. Если нет - пересоздать. Для пересоздания таблицы по хорошему нужен отдельный метод.
		 * 2. Проверить предыдущие значения в этой точке времени. Если они есть - изменить Using
		 * 3. Произвести запись
		 * 4. Проверить, есть ли в этой таблице записи позже записанной. Если их нет - мы должны обновить Initial значение в следующей (из существующих) таблице
		 */
		var table = await GetHistoryTableAsync(record.Date);

		// указываем, что предыдущие значения в этой точке времени устарели
		await table
			.Where(x => x.TagId == record.TagId && x.Date == record.Date)
			.Set(x => x.Using, TagUsing.Outdated)
			.UpdateAsync();

		// запись нового значения
		await table
			.Value(x => x.TagId, record.TagId)
			.Value(x => x.Date, record.Date)
			.Value(x => x.Text, record.Text)
			.Value(x => x.Number, record.Number)
			.Value(x => x.Quality, record.Quality)
			.Value(x => x.Using, TagUsing.Basic)
			.InsertAsync();

		// проверка, является ли новое значение последним в таблице
		// если да, мы должны обновить следующие Using = Initial по каскаду до последнего
		var valueAfterWrited = await table.Where(x => x.TagId == record.TagId && x.Date > record.Date).ToArrayAsync();
		if (valueAfterWrited.Length == 0)
		{
			var nextTablesDates = Cache.Tables
				.Where(x => x.Key > record.Date)
				.OrderBy(x => x.Key)
				.Select(x => new
				{
					Date = x.Key,
					Table = x.Value,
				})
				.ToList();

			foreach (var next in nextTablesDates)
			{
				// выгружаем значения
				// нам достаточно двух, Initial от прошлой таблицы и Basic за этот день
				var nextTable = await GetHistoryTableAsync(next.Date);
				var nextTableValues = await nextTable
					.Where(x => x.TagId == record.TagId)
					.OrderBy(x => x.Date)
					.Take(2)
					.ToListAsync();

				// проверяем, есть ли Initial значение, если да - обновляем, если нет - создаём
				if (nextTableValues.Any(x => x.Using == TagUsing.Initial))
				{
					await nextTable
						.Where(x => x.TagId == record.TagId && x.Using == TagUsing.Initial)
						.Set(x => x.Number, record.Number)
						.Set(x => x.Text, record.Text)
						.Set(x => x.Quality, record.Quality)
						.UpdateAsync();
				}
				else
				{
					await nextTable
						.Value(x => x.TagId, record.TagId)
						.Value(x => x.Date, next.Date)
						.Value(x => x.Number, record.Number)
						.Value(x => x.Text, record.Text)
						.Value(x => x.Quality, record.Quality)
						.Value(x => x.Using, TagUsing.Initial)
						.InsertAsync();
				}

				// проверяем, есть ли Basic записи в этой таблице, если есть - выходим из цикла
				if (nextTableValues.Any(x => x.Using == TagUsing.Basic))
				{
					break;
				}
			}
		}
	}

	#endregion


	#region Чтение значений

	public async Task<List<ValuesResponse>> GetValuesAsync(ValuesRequest[] requests)
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
							Using = x.Value.Using,
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
								Using = x.Using,
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
										Using = TagUsing.Aggregated,
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
										Using = TagUsing.Aggregated,
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

	public async Task<List<TagHistory>> ReadHistoryValuesAsync(
		int[] identifiers,
		DateTime old,
		DateTime young,
		int resolution = 0)
	{
		var sw = Stopwatch.StartNew();
		logger.LogInformation("Событие чтения архивных значений");

		var firstDate = old.Date;
		var lastDate = young.Date;
		var seekDate = firstDate;
		var values = new List<TagHistory>();

		Stopwatch d;
		d = Stopwatch.StartNew();

		var cachedTables = Cache.Tables
			.Select(x => new
			{
				Date = x.Key,
				Table = x.Value,
			})
			.ToArray();

		var tables = cachedTables
			.Where(x => x.Date >= firstDate && x.Date <= lastDate)
			.Union(cachedTables.Where(x => x.Date <= firstDate).OrderByDescending(x => x.Date).Take(1))
			.Union(cachedTables.Where(x => x.Date >= firstDate).OrderBy(x => x.Date).Take(1))
			.ToDictionary(x => x.Date, x => x.Table);

		d.Stop();
		logger.LogInformation("Список таблиц: {ms} мс", d.Elapsed.TotalMilliseconds);

		// выгружаем текущие значения
		d = Stopwatch.StartNew();
		do
		{
			if (tables.ContainsKey(seekDate))
			{
				var table = db.GetTable<TagHistory>().TableName(GetTableName(seekDate));

				try
				{
					values.AddRange([.. table
						.Where(x => identifiers.Contains(x.TagId))
						.Where(x => x.Date >= old)
						.Where(x => x.Date <= young)
						.Where(x => x.Using == TagUsing.Basic)
						.OrderBy(x => x.Date)]);
				}
				catch { }
			}

			seekDate = seekDate.AddDays(1);
		}
		while (seekDate <= lastDate);
		d.Stop();
		logger.LogInformation("Чтение значений: {ms} мс", d.Elapsed.TotalMilliseconds);

		// проверяем наличие значений на old

		d = Stopwatch.StartNew();
		var lost = identifiers
			.Except(values
				.Where(x => x.Date == old)
				.Select(x => x.TagId))
			.ToList();

		if (lost.Count > 0)
		{
			// если у нас есть непроинициализированные теги
			// нужно получить initial значения для них
			// берем ближайшую в прошлом таблицу
			var initialDate = tables.Keys
				.Where(x => x <= firstDate)
				.OrderByDescending(x => x)
				.DefaultIfEmpty(DateTime.MinValue)
				.FirstOrDefault();

			// если у нас не было подходящих таблиц в выборке, нужно их дозагрузить
			if (initialDate == DateTime.MinValue)
			{
				// ближайшая, где будут все нужные теги, это следующая существующая после old
				// если такой не будет, значит нам нужна последняя существующая перед old
				// разница в том, что в первом случае мы просто возьмём initial, а во втором рассчитаем их через группировку, что дороже
			}

			string initialTableName = GetTableName(initialDate);

			if (tables.ContainsKey(initialDate))
			{
				var initialTable = db.GetTable<TagHistory>().TableName(initialTableName);

				// определяем список тех значений, которым не хватает initial
				// и грузим их
				List<TagHistory> initial = await initialTable
					.Where(x => x != null && lost.Contains(x.TagId))
					.Where(x => x.Date <= old)
					.Where(x => x.Using == TagUsing.Initial || x.Using == TagUsing.Basic)
					.GroupBy(x => x.TagId)
					.Where(g => g.Count() > 0)
					.Select(g => g.OrderByDescending(x => x.Date).First())
					.ToListAsync();

				foreach (var value in initial)
				{
					if (value == null)
						continue;
					if (value.Date < old)
					{
						// обрезаем значение по временному окну, т.е. по old
						value.Date = old;
						value.Using = TagUsing.Continuous;
					}
					else if (value.Date > old)
					{
						// если мы обнаружили, что initial был уже после old, это значит, что тег был создан после old
						// для него мы делаем заглушку
						values.Add(LostTag(value.TagId));
					}
				}

				values.AddRange(initial);
			}
		}
		d.Stop();
		logger.LogInformation("Чтение предшествующих значений: {ms} мс", d.Elapsed.TotalMilliseconds);

		// заглушки, если значения так и не были проинициализированы
		d = Stopwatch.StartNew();
		lost = identifiers
			.Except(values
				.Where(x => x.Date == old)
				.Select(x => x.TagId))
			.ToList();

		var lostTags = await db.Tags
			.Where(x => lost.Contains(x.Id))
			.Select(x => x.Id)
			.ToListAsync();

		values.AddRange(lostTags.Select(LostTag));
		d.Stop();
		logger.LogInformation("Чтение отсутствующий значений: {ms} мс", d.Elapsed.TotalMilliseconds);

		// выполняем протяжку, если необходимо
		if (resolution > 0)
		{
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
								Using = TagUsing.Continuous,
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
		}

		sw.Stop();
		logger.LogInformation("Архивные данные прочитаны: [{tags}] тегов, [{values}] значений за {ms} мс", identifiers.Length, values.Count, sw.Elapsed.TotalMilliseconds);

		return values;

		TagHistory LostTag(int id) => new()
		{
			TagId = id,
			Date = old,
			Text = null,
			Number = null,
			Quality = TagQuality.Bad,
			Using = TagUsing.NotFound,
		};
	}

	#endregion


	public void Dispose()
	{
		db.Close();
		GC.SuppressFinalize(this);
	}
}
