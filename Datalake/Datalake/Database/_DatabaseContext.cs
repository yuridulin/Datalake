using Datalake.Enums;
using Datalake.Models;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Datalake.Database
{
	public class DatabaseContext : DataConnection
	{
		public static int CurrentSchemeVersion = V3.Migrator.Version;

		public static StartupConnection ConnectionSetup { get; set; }

		public DatabaseContext() : base(ConnectionSetup.Provider, ConnectionSetup.ConnectionString) { }

		public ITable<Settings> Settings
			=> this.GetTable<Settings>();

		public ITable<Log> Logs
			=> this.GetTable<Log>();

		public ITable<User> Users
			=> this.GetTable<User>();

		public ITable<Tag> Tags
			=> this.GetTable<Tag>();

		public ITable<TagHistory> TagsHistory
			=> this.GetTable<TagHistory>();

		public ITable<Source> Sources
			=> this.GetTable<Source>();

		public ITable<Block> Blocks
			=> this.GetTable<Block>();

		public ITable<Rel_Tag_Input> Rel_Tag_Input
			=> this.GetTable<Rel_Tag_Input>();

		public ITable<Rel_Block_Tag> Rel_Block_Tag
			=> this.GetTable<Rel_Block_Tag>();


		// методы

		public void Migrate()
		{
			int version = GetSchemeVersion();

			if (version < CurrentSchemeVersion)
			{
				if (version == 0) version = SetSchemeVersion(V1.Migrator.Migrate(this));
				if (version == 1) version = SetSchemeVersion(V2.Migrator.Migrate(this));
				if (version == 2) version = SetSchemeVersion(V3.Migrator.Migrate(this));
			}

			if (version != CurrentSchemeVersion)
			{
				throw new Exception("Миграция не выполнена!");
			}

			Log(new Log
			{
				Category = LogCategory.Database,
				Type = LogType.Success,
				Text = $"Схема базы данных приведена к актуальной версии: {version}"
			});

			CreateCache();
		}

		void CreateCache()
		{
			var tags = Tags
				.ToList();

			var provider = DataProvider.GetSchemaProvider();
			var schema = provider.GetSchema(this);

			Cache.Tables = schema.Tables
				.Where(t => t.TableName.StartsWith("TagsHistory_"))
				.Select(t => t.TableName)
				.ToList();

			var last = schema.Tables
				.Where(x => x.TableName.StartsWith("TagsHistory_"))
				.OrderByDescending(x => x.TableName)
				.Select(x => x.TableName)
				.FirstOrDefault();

			if (last == null)
			{
				Cache.Live = tags
					.ToDictionary(x => x.Id, x => DefaultValue(x));
			}
			else
			{
				var table = this.GetTable<TagHistory>().TableName(last);

				var values = table
					.ToList()
					.GroupBy(x => x.TagId)
					.Select(g => new
					{
						Id = g.Key,
						Value = g.OrderByDescending(x => x.Date).FirstOrDefault()
					})
					.ToList();

				Cache.Live = tags
					.ToDictionary(x => x.Id, x => values.FirstOrDefault(v => v.Id == x.Id)?.Value ?? DefaultValue(x));
			}

			Cache.Update();

			TagHistory DefaultValue(Tag tag) => new TagHistory
			{
				TagId = tag.Id,
				Date = DateTime.Now,
				Number = null,
				Quality = 0,
				Text = null,
				Type = tag.Type,
				Using = TagHistoryUse.Initial
			};
		}

		readonly string TagsHistoryName = "TagsHistory_";

		int GetSchemeVersion()
		{
			try
			{
				var version = Settings.Where(x => x.Key == "Version").Select(x => x.Value).FirstOrDefault();
				if (version == null) return 0;

				return int.TryParse(version, out int v) ? v : 0;
			}
			catch { return 0; }
		}

		int SetSchemeVersion(int version)
		{
			try
			{
				if (Settings.Any(x => x.Key == "Version"))
				{
					Settings.Where(x => x.Key == "Version").Set(x => x.Value, version.ToString()).Update();
				}
				else
				{
					Settings.Value(x => x.Key, "Version").Value(x => x.Value, version.ToString()).Insert();
				}
				return version;
			}
			catch { return 0; }
		}

		public void Log(Log log)
		{
			try
			{
				log.Date = DateTime.Now;
				this.Insert(log);

				Console.WriteLine($"{log.Date:dd.MM.yyyy HH:mm:ss} [{log.Category}] {log.Type} : {log.Text}{(log.Ref.HasValue ? $" | Id {log.Ref}" : "")}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[LOG] Error : {ex.Message}\n{ex.StackTrace}");
			}
		}

		string GetTableName(DateTime date) => TagsHistoryName + date.ToString("yyyy_MM_dd");

		List<DateTime> GetStoredDays()
		{
			return Cache.Tables
				.Select(x => DateTime.ParseExact(x.Replace(TagsHistoryName, ""), "yyyy_MM_dd", new CultureInfo("ru-RU")))
				.ToList();
		}

		public ITable<TagHistory> GetHistoryTable(DateTime seekDate)
		{
			string historyTableName = GetTableName(seekDate);

			if (!Cache.Tables.Any(x => x == historyTableName))
			{
				/*
				 * Определение новой временной таблицы
				 * 1. Создаём новую таблицу на нужную дату
				 * 2. Находим предшествующую по дате из доступных.
				 * 3. Определяем записанные в неё теги. Если таблицы нет, берем все теги из базы
				 * 
				 * Пометка на будущее - нам нужно делать воссоздание лишь тех тегов, что были добавлены в базу до этой даты.
				 * Поэтому нам нужно хранить дату создания тега, что приводит к миграциям
				 * Для миграций нужно знать, какая структура уже реализована в базе данных, что приводит к таблице настроек с версией схемы в ней
				 * 
				 * 4. Получаем последние значения для каждого тега. Если таблицы нет, берем null
				 * 5. Записываем эти значения в новую таблицу с Using = Initial
				 */
				var newTable = this.CreateTable<TagHistory>(historyTableName);

				var previousDate = GetStoredDays()
					.OrderByDescending(x => x)
					.DefaultIfEmpty(seekDate)
					.FirstOrDefault();

				if (previousDate != seekDate)
				{
					var initialValues = new List<TagHistory>();

					var previousTable = this.GetTable<TagHistory>().TableName(GetTableName(previousDate));

					var previousWritedTags = previousTable
						.GroupBy(x => x.TagId)
						.Select(g => g
							.OrderByDescending(x => x.Date)
							.FirstOrDefault() ?? new TagHistory { Number = null, Text = null }
						)
						.ToList()
						.Select(x => new TagHistory
						{
							Date = DateTime.Today,
							TagId = x.TagId,
							Number = x.Number,
							Text = x.Text,
							Type = x.Type,
							Quality = x.Quality,
							Using = TagHistoryUse.Initial,
						})
						.ToList();

					initialValues.AddRange(previousWritedTags);

					var notFoundTags = Tags
						.Select(x => x.Id)
						.Where(x => !initialValues.Any(v => v.TagId == x))
						.Select(x => new TagHistory
						{
							TagId = x,
							Date = DateTime.Today,
							Text = null,
							Number = null,
							Type = 0,
							Quality = TagQuality.Bad,
							Using = TagHistoryUse.Initial,
						})
						.ToList();

					initialValues.AddRange(notFoundTags);

					newTable.BulkCopy(initialValues);
				}

				Cache.Tables.Add(historyTableName);

				return newTable;
			}
			else
			{
				return this.GetTable<TagHistory>().TableName(historyTableName);
			}
		}

		public List<TagHistory> ReadHistory(int[] identifiers, DateTime old, DateTime young, int resolution = 0)
		{
			DateTime seek = old.Date;

			var values = new List<TagHistory>();

			// выгружаем записи из базы
			do
			{
				string tableName = GetTableName(seek);

				if (Cache.Tables.Contains(tableName))
				{
					var table = this.GetTable<TagHistory>().TableName(tableName);

					try
					{
						// собираем все подходящие значения
						values.AddRange(table
							.Where(x => identifiers.Contains(x.TagId))
							.Where(x => x.Date >= old)
							.Where(x => x.Date <= young)
							.Where(x => x.Using == TagHistoryUse.Basic)
							.OrderBy(x => x.Date)
							.ToList());

						// если это первая таблица в списке, собираем начальные значения
						if (seek == old.Date)
						{
							var initial = table
								.Where(x => identifiers.Contains(x.TagId))
								.Where(x => x.Date < old)
								.GroupBy(x => x.TagId)
								.Select(g => g.OrderByDescending(x => x.Date).FirstOrDefault())
								.ToList();

							foreach (var value in initial)
							{
								if (value.Date < old)
								{
									// обрезаем значение по временному окну, т.е. по old
									value.Date = old;
									value.Using = TagHistoryUse.Continuous;
								}
								else if (value.Date > old)
								{
									// если мы обнаружили, что initial был уже после old, это значит, что тег был создан после old
									// для него мы делаем заглушку
									values.Add(new TagHistory
									{
										TagId = value.TagId,
										Date = old,
										Text = null,
										Number = null,
										Type = value.Type,
										Quality = TagQuality.Bad,
										Using = TagHistoryUse.NotFound,
									});
								}
							}

							values.AddRange(initial);
						}
					}
					catch { }
				}

				seek = seek.AddDays(1);
			}
			while (seek <= young);

			// для тегов, по которым мы ничего не нашли, ставим заглушки
			var lostIdentifiers = identifiers.Except(values.Select(x => x.TagId)).ToList();

			if (lostIdentifiers.Count > 0)
			{
				var lostTags = Tags.Where(x => lostIdentifiers.Contains(x.Id)).ToList();

				values.AddRange(lostTags.Select(x => new TagHistory
				{
					TagId = x.Id,
					Date = old,
					Text = null,
					Number = null,
					Type = x.Type,
					Quality = TagQuality.Bad,
					Using = TagHistoryUse.NotFound,
				}));
			}

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
									Type = value.Type,
									Quality = value.Quality,
									Using = TagHistoryUse.Continuous,
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
		}

		public void WriteHistory(List<TagHistory> values)
		{
			foreach (var g in values.GroupBy(x => x.Date))
			{
				var groupValues = g.ToList();

				Cache.WriteMany(groupValues);
				GetHistoryTable(g.Key).BulkCopy(groupValues);
			}
		}

		public void WriteManual(TagHistory value)
		{
			/*
			 * Реализация:
			 * 1. Определить, существует ли таблица на дату записи. Если нет - пересоздать. Для пересоздания таблицы по хорошему нужен отдельный метод.
			 * 2. Проверить предыдущие значения в этой точке времени. Если они есть - изменить Using
			 * 3. Произвести запись
			 * 4. Проверить, есть ли в этой таблице записи позже записанной. Если их нет - мы должны обновить Initial значение в следующей (из существующих) таблице
			 */

			var table = GetHistoryTable(value.Date);

			var values = table
				.Where(x => x.TagId == value.TagId && x.Date >= value.Date)
				.ToList();

			// указываем, что предыдущие значения в этой точке времени устарели
			if (values.Any(x => x.Date == value.Date))
			{
				table
					.Where(x => x.TagId == value.TagId && x.Date == value.Date)
					.Set(x => x.Using, TagHistoryUse.Outdated)
					.Update(); 
			}

			// запись нового значения
			table
				.Value(x => x.TagId, value.TagId)
				.Value(x => x.Type, value.Type)
				.Value(x => x.Date, value.Date)
				.Value(x => x.Text, value.Text)
				.Value(x => x.Number, value.Number)
				.Value(x => x.Quality, value.Quality)
				.Value(x => x.Using, TagHistoryUse.Basic)
				.Insert();

			Cache.Write(value);

			// проверка, является ли новое значение последним в таблице
			// если да, мы должны обновить следующие Using = Initial по каскаду до последнего

			if (!values.Any(x => x.Date > value.Date))
			{
				var dates = GetStoredDays();

				var nextTablesDates = dates
					.Where(x => x > value.Date)
					.OrderBy(x => x)
					.ToList();

				foreach (var nextTableDate in nextTablesDates)
				{
					// выгружаем значения
					// нам достаточно двух, Initial от прошлой таблицы и Basic за этот день
					var nextTable = GetHistoryTable(nextTableDate);
					var nextTableValues = nextTable
						.Where(x => x.TagId == value.TagId)
						.OrderBy(x => x.Date)
						.Take(2)
						.ToList();

					// проверяем, есть ли Initial значение, если да - обновляем
					if (nextTableValues.Any(x => x.Using == TagHistoryUse.Initial))
					{
						nextTable
							.Where(x => x.TagId == value.TagId && x.Using == TagHistoryUse.Initial)
							.Set(x => x.Number, value.Number)
							.Set(x => x.Text, value.Text)
							.Set(x => x.Quality, value.Quality)
							.Update();
					}

					// проверяем, есть ли Basic записи в этой таблице, если есть - выходим из цикла
					if (nextTableValues.Any(x => x.Using == TagHistoryUse.Basic))
					{
						break;
					}
				}
			}
		}
	}
}
