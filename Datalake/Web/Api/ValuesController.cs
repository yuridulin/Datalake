using Datalake.Database;
using Datalake.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Datalake.Web.Api
{
	public class ValuesController : Controller
	{
		public object Live(string[] tags)
		{
			using (var db = new DatabaseContext())
			{
				var values = db.TagsLive
					.Where(x => tags.Length == 0 || tags.Contains(x.TagName))
					.ToList();

				return values;
			}
		}

		public object History(string[] tags, DateTime old, DateTime young, int resolution)
		{
			Console.WriteLine($"History for [{string.Join(", ", tags)}] from {old} to {young} with {resolution}");

			using (var db = new DatabaseContext())
			{
				// Выгружаем из базы записанные значения в диапазоне old .. young
				// Также выгружаем начальное значение до точки old:
				// 1) читаем id старшего значения,
				// 2) ищем значение с id меньше,
				// 3) добавляем как значение с временем old

				/* 
				 * ВАЖНО! Нужно сделать вставку промежуточных значений со специальным флагом, 
				 * чтобы использовать их как стартовое значение без необходимости обходить весь массив
				 * 
				 * При создании новой суточной таблицы так же делать стартовые значения всех тегов
				 */

				var data = db.TagsHistory
					.Where(x => tags.Contains(x.TagName))
					.Where(x => x.Date > old)
					.Where(x => x.Date <= young)
					.ToList();

				Console.WriteLine("Stored: " + data.Count);

				var previousIds = db.TagsHistory
					.Where(x => tags.Contains(x.TagName))
					.Where(x => x.Date <= old)
					.GroupBy(x => x.TagName)
					.Select(g => g.Select(x => x.Id).Max())
					.ToList();

				var previousValues = db.TagsHistory
					.Where(x => previousIds.Contains(x.Id))
					.ToList();

				var series = data
					.GroupBy(x => x.TagName)
					.Select(g => new TagValuesRange
					{
						TagName = g.Key,
						Values = g
							.Select(x => new TagValue
							{
								Id = x.Id,
								Date = x.Date,
								Number = x.Number,
								Text = x.Text
							})
							.ToList()
					})
					.ToList();

				Console.WriteLine("Series: " + series.Count);

				foreach (var range in series)
				{
					var previous = previousValues.FirstOrDefault(x => x.TagName == range.TagName);
					if (previous != null)
					{
						range.Values.Insert(0, new TagValue
						{
							Id = previous.Id,
							Date = old,
							Number = previous.Number,
							Text = previous.Text
						});
					}
				}

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
								Text = value.Text,
								Number = value.Number,
							});
						}

						range.Values = intervalValues;

						Console.WriteLine($"Range for {range.TagName} has {range.Values.Count}");
					}

					return series;
				}
			}
		}
	}
}
