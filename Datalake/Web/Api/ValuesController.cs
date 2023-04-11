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
				var dbTags = db.Tags
					.Where(x => tags.Length == 0 || tags.Contains(x.TagName))
					.ToList();

				var values = db.TagsLive
					.Where(x => tags.Length == 0 || tags.Contains(x.TagName))
					.ToList();

				var series = dbTags
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
								Quality = v.Quality
							})
							.ToList()
					})
					.ToList();

				return series;
			}
		}

		public object History(string[] tags, DateTime old, DateTime young, int resolution)
		{
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

				var dbTags = db.Tags
					.Where(x => tags.Contains(x.TagName))
					.ToList();

				var data = db.TagsHistory
					.Where(x => tags.Contains(x.TagName))
					.Where(x => x.Date > old)
					.Where(x => x.Date <= young)
					.OrderBy(x => x.Date)
					.ToList();

				var previousIds = db.TagsHistory
					.Where(x => tags.Contains(x.TagName))
					.Where(x => x.Date <= old)
					.GroupBy(x => x.TagName)
					.Select(g => g.Select(x => x.Id).Max())
					.ToList();

				var previousValues = db.TagsHistory
					.Where(x => previousIds.Contains(x.Id))
					.ToList();

				var series = new List<TagValuesRange>();

				foreach (var tag in dbTags)
				{
					Console.WriteLine($"Тег {tag.TagName} типа {tag.TagType}");

					var range = new TagValuesRange
					{
						TagName = tag.TagName,
						TagType = tag.TagType,
						Values = new List<TagValue>()
					};

					var previous = previousValues.FirstOrDefault(x => x.TagName == range.TagName);
					if (previous != null)
					{
						range.Values.Add(new TagValue
						{
							Id = previous.Id,
							Date = previous.Date,
							Value = previous.Value(range.TagType),
							Quality = previous.Quality
						});

						Console.WriteLine($"+ [{previous.Id}] {previous.Date}");
					}

					foreach (var x in data.Where(x => x.TagName == range.TagName))
					{
						range.Values.Add(new TagValue
						{
							Id = x.Id,
							Date = x.Date,
							Value = x.Value(range.TagType),
							Quality = x.Quality
						});

						Console.WriteLine($"+ [{x.Id}] {x.Date}");
					}

					series.Add(range);
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
								Value = value.Value,
								Quality = value.Quality
							});
						}

						range.Values = intervalValues;
					}

					return series;
				}
			}
		}
	}
}
