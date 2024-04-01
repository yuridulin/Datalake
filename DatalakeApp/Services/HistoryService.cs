using DatalakeDatabase;

namespace DatalakeApp.Services
{
	public class HistoryService(DatalakeContext db)
	{
		public static readonly string NamePrefix = "TagsHistory_";
		public static readonly string DateMask = "yyyy_MM_dd";
/*
		#region Манипулирование таблицами

		static string GetTableName(DateOnly date) => NamePrefix + date.ToString(DateMask);

		public async Task<ITable<TagHistory>> GetHistoryTableAsync(DateOnly date)
		{
			var chunk = await db.Chunks
				.Where(x => x.Date == date)
				.FirstOrDefaultAsync();

			if (chunk == null)
			{
				// создание новой таблицы в случае, если её не было
				var newTable = await db.CreateTableAsync<TagHistory>(GetTableName(date));

				// инициализация значений по последним из предыдущей таблицы
				var previousChunk = await db.Chunks
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

		public async Task WriteLiveValueAsync(TagHistory value)
		{
			var existValue = await db.TagsLive
				.Where(x => x.TagId == value.TagId)
				.FirstOrDefaultAsync();

			int count = 1;

			if (existValue == null)
			{
				count = await db.TagsLive
					.Value(x => x.TagId, value.TagId)
					.Value(x => x.Date, value.Date)
					.Value(x => x.Text, value.Text)
					.Value(x => x.Number, value.Number)
					.Value(x => x.Quality, value.Quality)
					.Value(x => x.Using, value.Using)
					.InsertAsync();
			}
			else if (existValue.Date < value.Date)
			{
				count = await db.TagsLive
					.Where(x => x.TagId == value.TagId)
					.Set(x => x.Date, value.Date)
					.Set(x => x.Text, value.Text)
					.Set(x => x.Number, value.Number)
					.Set(x => x.Quality, value.Quality)
					.Set(x => x.Using, value.Using)
					.UpdateAsync();
			}

			if (count == 0)
				throw new Exception($"Не удалось записать текущее значение для тега #{value.TagId}");
		}

		public async Task WriteLiveValuesAsync(IEnumerable<TagHistory> values)
		{
			var identifiers = values.Select(x => x.TagId).ToHashSet();

			var existValues = await db.TagsLive
				.Where(x => identifiers.Contains(x.TagId))
				.ToListAsync();

			int count = 0;

			var result = await db.TagsLive
				.BulkCopyAsync(values.Where(x => !identifiers.Contains(x.TagId)));

			count += (int)result.RowsCopied;

			foreach (var value in existValues.Where(x => identifiers.Contains(x.TagId)))
			{
				count += await db.TagsLive
					.Where(x => x.TagId == value.TagId)
					.Set(x => x.Date, value.Date)
					.Set(x => x.Text, value.Text)
					.Set(x => x.Number, value.Number)
					.Set(x => x.Quality, value.Quality)
					.Set(x => x.Using, value.Using)
					.UpdateAsync();
			}

			if (count != values.Count())
				throw new Exception($"Не удалось записать некоторые текущие значения: {values.Count() - count}");
		}

		public async Task WriteHistoryValueAsync(TagHistory record)
		{
			var table = await GetHistoryTableAsync(DateOnly.FromDateTime(record.Date));
			await WriteLiveValueAsync(record);
			await table.BulkCopyAsync([record]);
		}

		public async Task WriteHistoryValuesAsync(IEnumerable<TagHistory> records)
		{
			foreach (var g in records.GroupBy(x => x.Date))
			{
				var table = await GetHistoryTableAsync(DateOnly.FromDateTime(g.Key));
				var values = g.ToList();
				await WriteLiveValuesAsync(values);
				await table.BulkCopyAsync(values);
			}
		}

		public async Task WriteManualHistoryValueAsync(TagHistory record)
		{
			*//*
			 * Реализация:
			 * 1. Определить, существует ли таблица на дату записи. Если нет - пересоздать. Для пересоздания таблицы по хорошему нужен отдельный метод.
			 * 2. Проверить предыдущие значения в этой точке времени. Если они есть - изменить Using
			 * 3. Произвести запись
			 * 4. Проверить, есть ли в этой таблице записи позже записанной. Если их нет - мы должны обновить Initial значение в следующей (из существующих) таблице
			 *//*
			var recordDate = DateOnly.FromDateTime(record.Date);
			var table = await GetHistoryTableAsync(recordDate);

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
				List<TagHistoryChunk> nextTablesDates = await db.Chunks
					.Where(x => x.Date > recordDate)
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

		public async Task<List<TagHistory>> ReadLiveValuesAsync(int[] identifiers)
		{
			return await db.TagsLive
				.Where(x => identifiers.Contains(x.TagId))
				.ToListAsync();
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
			var oldDate = DateOnly.FromDateTime(old);
			var youngDate = DateOnly.FromDateTime(young);
			var seek = oldDate;
			var values = new List<TagHistory>();

			var tables = await db.Chunks
				.Where(x => x.Date >= oldDate && x.Date <= youngDate)
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
			while (seek <= youngDate);

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
					.Where(x => x <= oldDate)
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

		#endregion*/
	}
}
