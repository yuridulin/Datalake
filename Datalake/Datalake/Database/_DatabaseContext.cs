using Datalake.Database.Enums;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Datalake.Database
{
	public class DatabaseContext : DataConnection
	{
		public DatabaseContext() : base("Default") { }

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


		// методы расширения для взаимодействия с базой

		public void Recreate()
		{
			var sp = DataProvider.GetSchemaProvider();
			var dbSchema = sp.GetSchema(this);

			if (!dbSchema.Tables.Any(t => t.TableName == Tags.TableName))
			{
				this.CreateTable<Tag>();
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

		public List<TagHistory> ReadHistory(int[] identifiers, DateTime old, DateTime young, int resolution = 0)
		{
			DateTime seek = old.Date;

			var data = new List<TagHistory>();

			do
			{
				string tableName = $"TagsHistory_{seek:yyyy_MM_dd}";

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

					if (seek == old.Date)
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

				seek = seek.AddDays(1);
			}
			while (seek <= young);

			if (resolution > 0)
			{
				// Разбивка значений в массив с заданным шагом
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
							.First();

						value.Date = stepDate;
						history.Add(value);
					}
				}

				data = history;
			}

			return data;
		}

		public void WriteHistory(List<TagHistory> values)
		{
			string currentHistoryTableName = $"TagsHistory_{DateTime.Today:yyyy_MM_dd}";

			var provider = DataProvider.GetSchemaProvider();
			var schema = provider.GetSchema(this);
			ITable<TagHistory> current;

			foreach (var value in values)
			{
				Cache.Write(value);
			}

			if (!schema.Tables.Any(x => x.TableName == currentHistoryTableName))
			{
				this.CreateTable<TagHistory>(currentHistoryTableName);

				// Нужно попробовать прочитать предыдущие значения, чтобы вставить их в новую таблицу
				// Это - оптимизация для быстрого получения исходного значения при выборке среза

				var tags = Tags
					.Select(x => x.Id)
					.ToList();

				var previous = schema.Tables
					.Where(x => x.TableName.StartsWith("TagsHistory_") && x.TableName != currentHistoryTableName)
					.OrderByDescending(x => x.TableName)
					.FirstOrDefault();

				var initialValues = new List<TagHistory>();

				if (previous != null)
				{
					var previousTable = this.GetTable<TagHistory>().TableName(previous.TableName);

					var lastValues = previousTable
						.GroupBy(x => x.TagId)
						.Select(g => g.OrderByDescending(x => x.Date).FirstOrDefault())
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

					initialValues.AddRange(lastValues);
				}

				var notFoundTags = tags
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

				current = this.GetTable<TagHistory>().TableName(currentHistoryTableName);
				current.BulkCopy(initialValues);
			}
			else
			{
				current = this.GetTable<TagHistory>().TableName(currentHistoryTableName);
			}

			current.BulkCopy(values);
		}
	}
}
