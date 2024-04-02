using DatalakeDatabase;
using DatalakeDatabase.Enums;
using DatalakeDatabase.Models;
using LinqToDB;
using LinqToDB.Data;

namespace DatalakeApp.Services
{
	public class HistoryService(DatalakeContext db, CacheService cache)
	{
		public static readonly string NamePrefix = "TagsHistory_";
		public static readonly string DateMask = "yyyy_MM_dd";

		#region Манипулирование таблицами

		public DatalakeContext Db => db;

		static string GetTableName(DateTime date) => NamePrefix + date.ToString(DateMask);

		public async Task<ITable<TagHistory>> GetHistoryTableAsync(DateTime date)
		{
			var chunk = await db.TagHistoryChunks
				.Where(x => x.Date == date)
				.FirstOrDefaultAsync();

			if (chunk == null)
			{
				// создание новой таблицы в случае, если её не было
				var newTable = await db.CreateTableAsync<TagHistory>(GetTableName(date));

				// инициализация значений по последним из предыдущей таблицы
				var previousChunk = await db.TagHistoryChunks
					.Where(x => x.Date < date)
					.OrderByDescending(x => x)
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
							Date = DateTime.Today,
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
							Date = DateTime.Today,
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
					Date = date,
					Table = GetTableName(date)
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

		public void WriteLiveValue(TagHistory value)
		{
			cache.WriteValue(value);
		}

		public void WriteLiveValues(IEnumerable<TagHistory> values)
		{
			cache.WriteValues(values.ToArray());
		}

		public async Task WriteHistoryValueAsync(TagHistory record)
		{
			var table = await GetHistoryTableAsync(record.Date);
			WriteLiveValue(record);
			await table.BulkCopyAsync([record]);
		}

		public async Task WriteHistoryValuesAsync(IEnumerable<TagHistory> records)
		{
			foreach (var g in records.GroupBy(x => x.Date))
			{
				var table = await GetHistoryTableAsync(g.Key);
				var values = g.ToList();
				WriteLiveValues(values);
				await table.BulkCopyAsync(values);
			}
		}

		public async Task WriteManualHistoryValueAsync(TagHistory record)
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
			if (!await table.AnyAsync(x => x.TagId == record.TagId && x.Date > record.Date))
			{
				List<TagHistoryChunk> nextTablesDates = await db.TagHistoryChunks
					.Where(x => x.Date > record.Date)
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

		public List<TagHistory> ReadLiveValues(int[] identifiers)
		{
			return [.. cache.ReadValues(identifiers)];
		}

		// TODO если окажется, что этот метод вызывается только при запросе в контроллере API, логику оттуда нужно перенести сюда
		// Это верно и для остальных методов этого сервиса
		// Нужно несколько универсальных точек входа и выхода без привязки к деталям
		public async Task<List<TagHistory>> ReadHistoryValuesAsync(
			int[] identifiers,
			DateTime old,
			DateTime young,
			int resolution = 0)
		{
			var seek = old;
			var values = new List<TagHistory>();

			var tables = await db.TagHistoryChunks
				.Where(x => x.Date >= old && x.Date <= young)
				.ToDictionaryAsync(x => x.Date, x => x.Table);

			// выгружаем текущие значения
			do
			{
				if (tables.ContainsKey(seek))
				{
					var table = db.GetTable<TagHistory>().TableName(GetTableName(seek));

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

				seek = seek.AddDays(1);
			}
			while (seek <= young);

			// проверяем наличие значений на old
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
					.Where(x => x <= old)
					.OrderByDescending(x => x)
					.FirstOrDefault();

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
	}
}
