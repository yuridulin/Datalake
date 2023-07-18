using Datalake.Database.Enums;
using Datalake.Database.Models;
using Datalake.Workers.Cache;
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

		public ITable<TagLive> TagsLive
			=> this.GetTable<TagLive>();

		public ITable<Source> Sources
			=> this.GetTable<Source>();

		public ITable<Settings> Settings
			=> this.GetTable<Settings>();

		public ITable<Block> Blocks
			=> this.GetTable<Block>();

		public ITable<ProgramLog> ProgramLog
			=> this.GetTable<ProgramLog>();

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

			if (!dbSchema.Tables.Any(t => t.TableName == TagsLive.TableName))
			{
				this.CreateTable<TagLive>();
			}

			if (!dbSchema.Tables.Any(t => t.TableName == Sources.TableName))
			{
				this.CreateTable<Source>();
			}

			if (!dbSchema.Tables.Any(t => t.TableName == Settings.TableName))
			{
				this.CreateTable<Settings>();
			}

			if (!dbSchema.Tables.Any(t => t.TableName == Blocks.TableName))
			{
				this.CreateTable<Block>();
			}

			if (!dbSchema.Tables.Any(t => t.TableName == ProgramLog.TableName))
			{
				this.CreateTable<ProgramLog>();
			}

			if (!dbSchema.Tables.Any(t => t.TableName == Rel_Tag_Input.TableName))
			{
				this.CreateTable<Rel_Tag_Input>();
			}

			if (!dbSchema.Tables.Any(t => t.TableName == Rel_Block_Tag.TableName))
			{
				this.CreateTable<Rel_Block_Tag>();
			}

			foreach (var tag in TagsLive)
			{
				CacheWorker.Live[tag.TagId] = tag;
			}
		}

		public void SetUpdateDate()
		{
			if (Settings.Count() == 0)
			{
				this.Insert(new Settings
				{
					LastUpdate = DateTime.Now,
				});
			}
			else
			{
				Settings
					.Set(x => x.LastUpdate, DateTime.Now)
					.Update();
			}
		}

		public void Log(string module, string message, ProgramLogType type = ProgramLogType.Error)
		{
			ProgramLog
				.Value(x => x.Module, module)
				.Value(x => x.Message, message)
				.Value(x => x.Timestamp, DateTime.Now)
				.Value(x => x.Type, type)
				.Insert();

			Console.WriteLine("{0} {1} {2}\n\t{3}", DateTime.Now, module, type, message);
		}

		public DateTime GetUpdateDate()
		{
			return Settings.FirstOrDefault()?.LastUpdate ?? DateTime.MinValue;
		}

		public List<TagValuesRange> ReadLive(string[] tagNames)
		{
			var tags = Tags
					.Where(x => tagNames.Length == 0 || tagNames.Contains(x.Name))
					.ToList();

			var ids = tags.Select(x => x.Id).ToList();

			var values = TagsLive
				.Where(x => tagNames.Length == 0 || ids.Contains(x.TagId))
				.ToList();

			var series = tags
				.Select(x => new TagValuesRange
				{
					TagType = x.Type,
					TagName = x.Name,
					Values = values
						.Where(v => x.Id == v.TagId)
						.Select(v => new TagValue
						{
							Id = 0,
							Date = v.Date,
							Value = v.Value(x.Type),
							Quality = v.Quality,
							Using = TagHistoryUse.Basic
						})
						.ToList()
				})
				.ToList();

			return series;
		}

		public List<TagValuesRange> ReadHistory(string[] tagNames, DateTime old, DateTime young, int resolution)
		{
			var tags = Tags
				.Where(x => tagNames.Contains(x.Name))
				.ToList();

			var ids = tags.Select(x => x.Id).ToList();

			DateTime seek = old.Date;

			var data = new List<TagHistory>();

			do
			{
				string tableName = $"TagsHistory_{seek:yyyy_MM_dd}";

				var table = this.GetTable<TagHistory>().TableName(tableName);

				List<TagHistory> chunk;
				try
				{
					chunk = table
						.Where(x => ids.Contains(x.TagId))
						.Where(x => x.Date >= old)
						.Where(x => x.Date <= young)
						.Where(x => x.Using == TagHistoryUse.Basic)
						.OrderBy(x => x.Date)
						.ToList();

					if (seek == old.Date)
					{
						// Проверка, нужно ли загружать начальные значения
						var notInitiatedTags = tags
							.Where(x => !chunk.Any(v => v.Date == old))
							.ToList();

						var previousIds = table
							.Where(x => ids.Contains(x.TagId))
							.Where(x => x.Date <= old)
							.GroupBy(x => x.TagId)
							.Select(g => g.Select(x => x.Id).Max())
							.ToList();

						var previousValues = table
							.Where(x => previousIds.Contains(x.Id))
							.Select(x => new TagHistory
							{
								Id = x.Id,
								TagId = x.TagId,
								Date = old,
								Text = x.Text,
								Number = x.Number,
								Quality = x.Quality,
								Using = TagHistoryUse.Initial
							})
							.ToList();

						data.AddRange(previousValues);
					}

					data.AddRange(chunk);
				}
				catch
				{
					Console.WriteLine("Таблица не найдена");
				}
				finally
				{
					seek = seek.AddDays(1);
				}
			}
			while (seek <= young);

			var series = tags
				.Select(tag => new TagValuesRange
				{
					TagName = tag.Name,
					TagType = tag.Type,
					Values = data
						.Where(x => x.TagId == tag.Id)
						.OrderBy(x => x.Date)
						.Select(x => new TagValue
						{
							Id = x.Id,
							Date = x.Date,
							Value = x.Value(tag.Type),
							Quality = x.Quality,
							Using = x.Using,
						})
						.ToList()
				})
				.ToList();

			if (resolution == 0)
			{
				// Выдача массива значений по изменению
				int id = 0;

				foreach (var range in series)
				{
					foreach (var value in range.Values)
					{
						value.Id = id++;
					}
				}

				return series;
			}
			else
			{
				// Разбивка значений в массив с заданным шагом

				var timeRange = (young - old).TotalMilliseconds;

				foreach (var range in series)
				{
					var intervalValues = new List<TagValue>();

					DateTime stepDate;
					TagValue stepValue = range.Values.First();

					for (double i = 0; i < timeRange; i += resolution)
					{
						stepDate = old.AddMilliseconds(i);

						var value = range.Values
							.Where(x => x.Date <= stepDate)
							.OrderByDescending(x => x.Date)
							.First();

						intervalValues.Add(new TagValue
						{
							Id = (long)i,
							Date = stepDate,
							Value = value.Value,
							Quality = value.Quality,
							Using = value.Using
						});
					}

					range.Values = intervalValues;
				}

				return series;
			}
		}

		public void WriteToHistory(TagHistory tag)
		{
			string currentHistoryTableName = $"TagsHistory_{DateTime.Today:yyyy_MM_dd}";

			try
			{
				// Запись в таблицу текущих значений в базе
				TagsLive
					.Where(x => x.TagId == tag.TagId)
					.Set(x => x.Date, tag.Date)
					.Set(x => x.Text, tag.Text)
					.Set(x => x.Number, tag.Number)
					.Set(x => x.Quality, tag.Quality)
					.Update();

				// Запись в таблицу текущих значений в памяти (нужно для оптимизации вычисления вычисляемых тегов)
				CacheWorker.Live[tag.TagId] = tag;

				// Запись в таблицу "Сегодня"
				Write(currentHistoryTableName);
			}
			catch
			{
				// Таблицы "Сегодня" нет, создаём, вписываем предыдущие значения, ещё раз пробуем записать
				var provider = DataProvider.GetSchemaProvider();
				var schema = provider.GetSchema(this);

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

						var lastIds = previousTable
							.GroupBy(x => x.TagId)
							.Select(g => g.Select(x => x.Id).Max())
							.ToList();

						var lastValues = previousTable
							.Where(x => lastIds.Contains(x.Id))
							.Select(x => new TagHistory
							{
								Date = DateTime.Today,
								TagId = x.TagId,
								Number = x.Number,
								Text = x.Text,
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
							Quality = TagQuality.Bad,
							Using = TagHistoryUse.Initial,
						})
						.ToList();

					initialValues.AddRange(notFoundTags);

					var currentTable = this.GetTable<TagHistory>().TableName(currentHistoryTableName);
					currentTable.BulkCopy(initialValues);

					Write(currentHistoryTableName);
				}
			}

			void Write(string tableName)
			{
				var table = this.GetTable<TagHistory>().TableName(tableName);

				table
					.Value(x => x.TagId, tag.TagId)
					.Value(x => x.Date, tag.Date)
					.Value(x => x.Text, tag.Text)
					.Value(x => x.Number, tag.Number)
					.Value(x => x.Quality, tag.Quality)
					.Value(x => x.Using, TagHistoryUse.Basic)
					.Insert();
			}
		}
	}
}
