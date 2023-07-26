using Datalake.Database;
using Datalake.Database.Models;
using Datalake.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Datalake.Web.Api
{
	public class ValuesController : Controller
	{
		public object LiveById(int[] id)
		{
			return id.ToDictionary(x => x, x => Cache.Read(x));
		}

		public object FlatHistory(string[] tags, bool live, DateTime old, DateTime young, int resolution)
		{
			using (var db = new DatabaseContext())
			{
				if (live)
				{
					var ids = db.Tags
						.Where(x => tags.Contains(x.Name))
						.ToList();

					return ids
						.Select(x => new
						{
							Tag = x,
							Value = Cache.Read(x.Id),
						})
						.Select(x => new FlatTagValue
						{
							Date = x.Value.Date,
							Value = x.Value.Value,
							Quality = x.Value.Quality,
							TagName = x.Tag.Name,
							Using = x.Value.Using,
						})
						.ToList();
				}
				else
				{
					var history = db.ReadHistory(tags, old, young, resolution);

					var flat = new List<FlatTagValue>();

					foreach (var range in history)
					{
						foreach (var value in range.Values)
						{
							flat.Add(new FlatTagValue
							{
								Date = value.Date,
								Value = value.Value,
								Quality = value.Quality,
								TagName = range.TagName,
								Using = value.Using,
							});
						}
					}

					return flat.OrderBy(x => x.TagName).ThenByDescending(x => x.Date).ToList();
				}
			}
		}
	}
}
