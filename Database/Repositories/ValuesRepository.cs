using Datalake.ApiClasses.Constants;
using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Exceptions;
using Datalake.ApiClasses.Models.Values;
using Datalake.Database.Extensions;
using Datalake.Database.Models;
using LinqToDB;
using LinqToDB.Data;

namespace Datalake.Database.Repositories;

public class ValuesRepository(DatalakeContext db) : IDisposable
{
	public static readonly string NamePrefix = "TagsHistory_";
	public static readonly string DateMask = "yyyy_MM_dd";


	#region Манипулирование таблицами

	static string GetTableName(DateOnly date) => NamePrefix + date.ToString(DateMask);

	async Task<ITable<TagHistory>> GetHistoryTableAsync(DateTime date)
	{
		return await GetHistoryTableAsync(DateOnly.FromDateTime(date));
	}

	async Task<ITable<TagHistory>> GetHistoryTableAsync(DateOnly seekDate)
	{
		var chunk = await db.TagHistoryChunks
			.Where(x => x.Date == seekDate)
			.FirstOrDefaultAsync();

		if (chunk == null)
		{
			// создание новой таблицы в случае, если её не было
			var newTable = await db.CreateTableAsync<TagHistory>(GetTableName(seekDate));

			// инициализация значений по последним из предыдущей таблицы
			var previousChunk = await db.TagHistoryChunks
				.Where(x => x.Date < seekDate)
				.OrderByDescending(x => x.Date)
				.FirstOrDefaultAsync();

			if (previousChunk != null)
			{
				var initialValues = new List<TagHistory>();

				var previousTable = db.GetTable<TagHistory>().TableName(previousChunk.Table);

				var previousWritedRows = await previousTable
					.GroupBy(x => x.TagId)
					.Select(g => g
						.OrderByDescending(x => x.Date)
						.FirstOrDefault() ?? new TagHistory { Number = null, Text = null }
					)
					.ToListAsync();

				var previousWritedTags = previousWritedRows
					.Select(x => new TagHistory
					{
						Date = seekDate.ToDateTime(TimeOnly.MinValue),
						TagId = x.TagId,
						Number = x.Number,
						Text = x.Text,
						Quality = x.Quality,
						Using = TagUsing.Initial,
					})
					.ToList();

				initialValues.AddRange(previousWritedTags);

				var notFoundTags = await db.Tags
					.Select(x => x.Id)
					.Where(x => !initialValues.Any(v => v.TagId == x))
					.Select(x => new TagHistory
					{
						TagId = x,
						Date = seekDate.ToDateTime(TimeOnly.MinValue),
						Text = null,
						Number = null,
						Quality = TagQuality.Bad,
						Using = TagUsing.Initial,
					})
					.ToListAsync();

				initialValues.AddRange(notFoundTags);

				await newTable.BulkCopyAsync(initialValues);
			}

			await db.InsertAsync(new TagHistoryChunk
			{
				Date = seekDate,
				Table = GetTableName(seekDate)
			});

			return newTable;
		}
		else
		{
			return db.GetTable<TagHistory>().TableName(GetTableName(chunk.Date));
		}
	}

	#endregion


	#region Запись значений

	/// <summary>
	/// Запись новых значений, с обновлением текущих при необходимости
	/// </summary>
	/// <param name="requests">Список запросов на запись</param>
	/// <returns>Ответ со списком значений, как при чтении</returns>
	/// <exception cref="NotFoundException">Тег не найден</exception>
	public async Task<List<ValuesTagResponse>> WriteValuesAsync(ValueWriteRequest[] requests)
	{
		List<ValuesTagResponse> responses = [];
		List<TagHistory> recordsToSimpleWrite = [];

		using var transaction = await db.BeginTransactionAsync();

		foreach (var writeRequest in requests)
		{
			Tag tag;
			if (writeRequest.Guid != null)
			{
				tag = await db.Tags
					.Where(x => x.GlobalGuid == writeRequest.Guid)
					.FirstOrDefaultAsync()
					?? throw new NotFoundException($"тег [{writeRequest.Guid}]");
			}
			else if (writeRequest.Name != null)
			{
				tag = await db.Tags
					.Where(x => x.Name == writeRequest.Name)
					.FirstOrDefaultAsync()
					?? throw new NotFoundException($"тег [{writeRequest.Name}]");
			}
			else if (writeRequest.Id != null)
			{
				tag = await db.Tags
					.Where(x => x.Id == writeRequest.Id)
					.FirstOrDefaultAsync()
					?? throw new NotFoundException($"тег [{writeRequest.Id}]");
			}
			else
			{
				continue;
			}

			var record = tag.ToHistory(writeRequest.Value, writeRequest.Quality);
			record.Date = writeRequest.Date ?? DateTime.Now;
			record.Using = TagUsing.Basic;

			await UpdateOrCreateLiveValueAsync(record);

			if (tag.SourceId == (int)CustomSource.Manual)
			{
				await WriteManualHistoryValueAsync(record);
			}
			else
			{
				recordsToSimpleWrite.Add(record);
			}

			responses.Add(new ValuesTagResponse
			{
				Id = tag.Id,
				Guid = tag.GlobalGuid,
				Name = tag.Name,
				Type = tag.Type,
				Values = [
					new ValueRecord
					{
						Date = record.Date,
						DateString = record.Date.ToString(DateFormats.HierarchicalWithMilliseconds),
						Quality = record.Quality,
						Value = record.GetTypedValue(tag.Type),
						Using = record.Using,
					}
				]
			});
		}

		await WriteHistoryValuesAsync(recordsToSimpleWrite);

		await transaction.CommitAsync();

		return responses;
	}

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

		await UpdateOrCreateLiveValueAsync(record);
		await WriteHistoryValueAsync(record);
	}

	async Task UpdateOrCreateLiveValueAsync(TagHistory record)
	{
		var lastLive = await db.TagsLive
			.Where(x => x.TagId == record.TagId)
			.Select(x => new { x.Date })
			.FirstOrDefaultAsync();

		if (lastLive == null)
		{
			await db.InsertAsync(record);
		}
		else if (lastLive.Date < record.Date)
		{
			await db.TagsLive
				.Where(x => x.TagId == record.TagId)
				.Set(x => x.Text, record.Text)
				.Set(x => x.Number, record.Number)
				.Set(x => x.Date, record.Date)
				.Set(x => x.Quality, record.Quality)
				.Set(x => x.Using, record.Using)
				.UpdateAsync();
		}
	}

	async Task WriteHistoryValuesAsync(IEnumerable<TagHistory> records)
	{
		if (!records.Any())
			return;

		foreach (var g in records.GroupBy(x => x.Date.Date))
		{
			var table = await GetHistoryTableAsync(g.Key);
			await table.BulkCopyAsync(records);
		}
	}

	async Task WriteHistoryValueAsync(TagHistory record)
	{
		var table = await GetHistoryTableAsync(record.Date);
		await table.BulkCopyAsync([record]);
	}

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
			List<TagHistoryChunk> nextTablesDates = await db.TagHistoryChunks
				.Where(x => x.Date > DateOnly.FromDateTime(record.Date))
				.OrderBy(x => x.Date)
				.ToListAsync();

			foreach (TagHistoryChunk next in nextTablesDates)
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
						.Value(x => x.Date, next.Date.ToDateTime(TimeOnly.MinValue))
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
		using var transaction = await db.BeginTransactionAsync();

		List<ValuesResponse> responses = [];

		foreach (var request in requests)
		{
			if (request.Tags?.Length == 0 && request.TagNames?.Length == 0 && request.TagId?.Length == 0)
				continue;

			var response = new ValuesResponse
			{
				RequestKey = request.RequestKey,
				Tags = []
			};
			var info = await ReadTagsInfoAsync(request.Tags, request.TagNames, request.TagId);

			DateTime exact = request.Exact ?? DateTime.Now;
			DateTime old, young;
			List<TagHistory> databaseValues;

			// Если не указывается ни одна дата, выполняется получение текущих значений. Не убирать!
			if (!request.Exact.HasValue && !request.Old.HasValue && !request.Young.HasValue)
			{
				databaseValues = await ReadLiveValuesAsync(info);
			}
			else
			{
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

				databaseValues = await ReadHistoryValuesAsync(info, old, young, resolution: Math.Max(0, request.Resolution ?? 0));
			}

			// сборка ответа, агрегация по необходимости
			foreach (var databaseValueGroup in databaseValues.GroupBy(x => x.TagId))
			{
				var tagInfo = info[databaseValueGroup.Key];

				if (request.Func == AggregationFunc.List)
				{
					response.Tags.Add(new ValuesTagResponse
					{
						Id = tagInfo.Id,
						Guid = tagInfo.Guid,
						Name = tagInfo.TagName,
						Type = tagInfo.TagType,
						Values = [.. databaseValueGroup
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
					var values = databaseValueGroup
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
							Name = tagInfo.TagName,
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
							Name = tagInfo.TagName,
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

			// дописываем информацию о тех тегах, которые не были найдены
			var notFoundTags = request.Tags != null ? request.Tags.Except(info.Values.Select(v => v.Guid)) : [];
			foreach (var guid in notFoundTags)
			{
				response.Tags.Add(new ValuesTagResponse
				{
					Id = info.Values.FirstOrDefault(v => v.Guid == guid)?.Id ?? -1,
					Guid = guid,
					Name = info.Values.FirstOrDefault(v => v.Guid == guid)?.TagName ?? string.Empty,
					Type = TagType.String,
					Values = [
						new ValueRecord
						{
							Date = exact,
							DateString = exact.ToString(DateFormats.HierarchicalWithMilliseconds),
							Quality = TagQuality.Bad_NoValues,
							Using = TagUsing.NotFound,
							Value = 0,
						}
					]
				});
			}

			responses.Add(response);
		}

		await transaction.CommitAsync();

		return responses;
	}

	async Task<Dictionary<int, ValueTagInfo>> ReadTagsInfoAsync(Guid[]? identifiers, string[]? names, int[]? id)
	{
		var info = await db.Tags
			.Where(x => (identifiers != null && identifiers.Contains(x.GlobalGuid))
				|| (names != null && names.Contains(x.Name))
				|| (id != null && id.Contains(x.Id))
			)
			.ToDictionaryAsync(x => x.Id, x => new ValueTagInfo
			{
				Id = x.Id,
				Guid = x.GlobalGuid,
				TagName = x.Name,
				TagType = x.Type,
			});

		return info;
	}

	async Task<List<TagHistory>> ReadLiveValuesAsync(
		Dictionary<int, ValueTagInfo> info)
	{
		var values = await db.TagsLive
			.Where(x => info.Keys.Contains(x.TagId))
			.ToListAsync();

		return values;
	}

	async Task<List<TagHistory>> ReadHistoryValuesAsync(
		Dictionary<int, ValueTagInfo> info,
		DateTime old,
		DateTime young,
		int resolution = 0)
	{
		var firstDate = DateOnly.FromDateTime(old);
		var lastDate = DateOnly.FromDateTime(young);
		var seekDate = firstDate;
		var values = new List<TagHistory>();

		var tables = await db.TagHistoryChunks
			.Where(x => x.Date >= firstDate && x.Date <= lastDate)
			.Union(db.TagHistoryChunks.Where(x => x.Date <= firstDate).OrderByDescending(x => x.Date).Take(1))
			.Union(db.TagHistoryChunks.Where(x => x.Date >= firstDate).OrderBy(x => x.Date).Take(1))
			.ToDictionaryAsync(x => x.Date, x => x.Table);

		// выгружаем текущие значения
		do
		{
			if (tables.ContainsKey(seekDate))
			{
				var table = db.GetTable<TagHistory>().TableName(GetTableName(seekDate));

				try
				{
					values.AddRange([.. table
						.Where(x => info.Keys.Contains(x.TagId))
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

		// проверяем наличие значений на old
		var lost = info.Keys
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
				.DefaultIfEmpty(DateOnly.MinValue)
				.FirstOrDefault();

			// если у нас не было подходящих таблиц в выборке, нужно их дозагрузить
			if (initialDate == DateOnly.MinValue)
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

		// заглушки, если значения так и не были проинициализированы
		lost = info.Keys
			.Except(values
				.Where(x => x.Date == old)
				.Select(x => x.TagId))
			.ToList();

		var lostTags = await db.Tags
			.Where(x => lost.Contains(x.Id))
			.Select(x => x.Id)
			.ToListAsync();

		values.AddRange(lostTags.Select(LostTag));

		// выполняем протяжку, если необходимо
		if (resolution > 0)
		{
			var timeRange = (young - old).TotalMilliseconds;
			var continuous = new List<TagHistory>();
			DateTime stepDate;

			for (double i = 0; i < timeRange; i += resolution)
			{
				stepDate = old.AddMilliseconds(i);

				foreach (var id in info.Keys)
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
