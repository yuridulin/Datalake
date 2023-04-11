using Datalake.Collector.Models;
using Datalake.Database;
using Datalake.Database.Enums;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Datalake.Collector
{
	public static class CollectorExtensions
	{
		public static void WriteToHistory(this DatabaseContext db, string tagName, DateTime date, string text, decimal? number, short quality)
		{
			string currentHistoryTableName = $"TagsHistory_{DateTime.Today:yyyy_MM_dd}";

			try
			{
				// Запись в таблицу текущих значений
				db.TagsLive
					.Where(x => x.TagName == tagName)
					.Set(x => x.Date, date)
					.Set(x => x.Text, text)
					.Set(x => x.Number, number)
					.Set(x => x.Quality, (TagQuality)quality)
					.Update();

				// Запись в таблицу "Сегодня"
				Write(currentHistoryTableName);
			}
			catch
			{
				// Таблицы "Сегодня" нет, создаём, вписываем предыдущие значения, ещё раз пробуем записать
				var provider = db.DataProvider.GetSchemaProvider();
				var schema = provider.GetSchema(db);

				if (!schema.Tables.Any(x => x.TableName == currentHistoryTableName))
				{
					db.CreateTable<TagHistory>(currentHistoryTableName);

					// Нужно попробовать прочитать предыдущие значения, чтобы вставить их в новую таблицу
					// Это - оптимизация для быстрого получения исходного значения при выборке среза

					var tags = db.Tags
						.Select(x => x.TagName)
						.ToList();

					var previous = schema.Tables
						.Where(x => x.TableName.StartsWith("TagsHistory_") && x.TableName != currentHistoryTableName)
						.OrderByDescending(x => x.TableName)
						.FirstOrDefault();

					var initialValues = new List<TagHistory>();

					if (previous != null)
					{
						var previousTable = db.GetTable<TagHistory>().TableName(previous.TableName);

						var lastIds = previousTable
							.GroupBy(x => x.TagName)
							.Select(g => g.Select(x => x.Id).Max())
							.ToList();

						var lastValues = previousTable
							.Where(x => lastIds.Contains(x.Id))
							.Select(x => new TagHistory
							{
								Date = DateTime.Today,
								TagName = x.TagName,
								Number = x.Number,
								Text = x.Text,
								Quality = x.Quality,
								Using = TagHistoryUse.Initial,
							})
							.ToList();

						initialValues.AddRange(lastValues);
					}

					var notFoundTags = tags
						.Where(x => !initialValues.Any(v => v.TagName == x))
						.Select(x => new TagHistory
						{
							TagName = x,
							Date = DateTime.Today,
							Text = null,
							Number = null,
							Quality = TagQuality.Bad,
							Using = TagHistoryUse.Initial,
						})
						.ToList();

					initialValues.AddRange(notFoundTags);

					var currentTable = db.GetTable<TagHistory>().TableName(currentHistoryTableName);
					currentTable.BulkCopy(initialValues);

					Write(currentHistoryTableName);
				}
			}

			void Write(string tableName)
			{
				var table = db.GetTable<TagHistory>().TableName(tableName);

				table
					.Value(x => x.TagName, tagName)
					.Value(x => x.Date, date)
					.Value(x => x.Text, text)
					.Value(x => x.Number, number)
					.Value(x => x.Quality, (TagQuality)quality)
					.Value(x => x.Using, TagHistoryUse.Basic)
					.Insert();
			}
		}

		public static List<TagValuesRange> ReadLive(this DatabaseContext db, string[] tagNames)
		{
			var tags = db.Tags
					.Where(x => tagNames.Length == 0 || tagNames.Contains(x.TagName))
					.ToList();

			var values = db.TagsLive
				.Where(x => tagNames.Length == 0 || tagNames.Contains(x.TagName))
				.ToList();

			var series = tags
				.Select(x => new TagValuesRange
				{
					TagType = x.TagType,
					TagName = x.TagName,
					Values = values
						.Where(v => x.TagName == v.TagName)
						.Select(v => new TagValue
						{
							Id = 0,
							Date = v.Date,
							Value = v.Value(x.TagType),
							Quality = v.Quality,
							Using = TagHistoryUse.Basic
						})
						.ToList()
				})
				.ToList();

			return series;
		}

		public static List<TagValuesRange> ReadHistory(this DatabaseContext db, string[] tagNames, DateTime old, DateTime young, int resolution)
		{
			var tags = db.Tags
				.Where(x => tagNames.Contains(x.TagName))
				.ToList();

			DateTime seek = old.Date;

			var data = new List<TagHistory>();

			do
			{
				string tableName = $"TagsHistory_{seek:yyyy_MM_dd}";

				var table = db.GetTable<TagHistory>().TableName(tableName);

				var chunk = table
					.Where(x => tagNames.Contains(x.TagName))
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
						.Where(x => tagNames.Contains(x.TagName))
						.Where(x => x.Date <= old)
						.GroupBy(x => x.TagName)
						.Select(g => g.Select(x => x.Id).Max())
						.ToList();

					var previousValues = table
						.Where(x => previousIds.Contains(x.Id))
						.Select(x => new TagHistory 
						{
							Id = x.Id,
							TagName = x.TagName,
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

				seek = seek.AddDays(1);
			}
			while (seek <= young);

			var series = tags
				.Select(tag => new TagValuesRange
				{
					TagName = tag.TagName,
					TagType = tag.TagType,
					Values = data
						.Where(x => x.TagName == tag.TagName)
						.OrderBy(x => x.Date)
						.Select(x => new TagValue
						{
							Id = x.Id,
							Date = x.Date,
							Value = x.Value(tag.TagType),
							Quality = x.Quality,
							Using = x.Using,
						})
						.ToList()
				})
				.ToList();

			if (resolution == 0)
			{
				// Выдача массива значений по изменению

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
	}
}
