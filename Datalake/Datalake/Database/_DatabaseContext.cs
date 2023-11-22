using Datalake.Enums;
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
		#if DEBUG
		static string connString = "Debug";
		#else
		static string connString = "Release";
		#endif

		public DatabaseContext() : base(connString) { }

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

		public void Recreate()
		{
			var sp = DataProvider.GetSchemaProvider();
			var dbSchema = sp.GetSchema(this);

			if (!dbSchema.Tables.Any(t => t.TableName == Tags.TableName))
			{
				this.CreateTable<Tag>();
			}
			else
			{
				Tags.Where(x => x.IsCalculating).Set(x => x.SourceId, CustomSourcesIdentity.Calculated).Update();
				Tags.Where(x => x.Name == "INSERTING").Delete();
			}

			if (!dbSchema.Tables.Any(t => t.TableName == Sources.TableName))
			{
				this.CreateTable<Source>();
			}

			if (!dbSchema.Tables.Any(t => t.TableName == Blocks.TableName))
			{
				this.CreateTable<Block>();
			}

			if (!dbSchema.Tables.Any(t => t.TableName == Rel_Tag_Input.TableName))
			{
				this.CreateTable<Rel_Tag_Input>();
			}

			if (!dbSchema.Tables.Any(t => t.TableName == Rel_Block_Tag.TableName))
			{
				this.CreateTable<Rel_Block_Tag>();
			}

			Cache.Tables = dbSchema.Tables
				.Where(t => t.TableName.StartsWith("TagsHistory_"))
				.Select(t => t.TableName)
				.ToList();

			CreateCache();

			void CreateCache()
			{
				var tags = Tags
					.ToList();

				var provider = DataProvider.GetSchemaProvider();
				var schema = provider.GetSchema(this);

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
		}

		readonly string TagsHistoryName = "TagsHistory_";

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

			var data = new List<TagHistory>();

			do
			{
				string tableName = GetTableName(seek);

				if (Cache.Tables.Contains(tableName))
				{
					var table = this.GetTable<TagHistory>().TableName(tableName);

					try
					{
						var chunk = table
							.Where(x => identifiers.Contains(x.TagId))
							.Where(x => x.Date >= old)
							.Where(x => x.Date <= young)
							.Where(x => x.Using == TagHistoryUse.Basic)
							.OrderBy(x => x.Date)
							.ToList();

						var min = chunk.Select(x => x.Date).Min();

						if (seek == old.Date && min > old)
						{
							var previousValues = table
								.Where(x => identifiers.Contains(x.TagId))
								.AsEnumerable()
								.GroupBy(x => x.TagId)
								.Select(g => g.OrderByDescending(x => x.Date).FirstOrDefault())
								.ToList();

							foreach (var value in previousValues)
							{
								value.Date = old;
								value.Using = TagHistoryUse.Initial;
							}

							data.AddRange(previousValues);
						}

						data.AddRange(chunk);
					}
					catch { }
				}

				seek = seek.AddDays(1);
			}
			while (seek <= young);

			if (resolution > 0)
			{
				var timeRange = (young - old).TotalMilliseconds;
				var history = new List<TagHistory>();
				DateTime stepDate;

				for (double i = 0; i < timeRange; i += resolution)
				{
					stepDate = old.AddMilliseconds(i);

					foreach (var id in identifiers)
					{
						var value = data
							.Where(x => x.TagId == id)
							.Where(x => x.Date <= stepDate)
							.OrderByDescending(x => x.Date)
							.FirstOrDefault();

						if (value != null)
						{
							if (value.Date != stepDate)
							{
								value.Date = stepDate;
								value.Using = TagHistoryUse.Continuous;
							}

							history.Add(value);
						}
					}
				}

				data = history;
			}

			return data;
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
