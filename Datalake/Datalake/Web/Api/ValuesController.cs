using Datalake.Database;
using Datalake.Database.Models;
using Datalake.Web.Models;
using Datalake.Workers.Cache;
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
				var model = db.ReadLive(tags);

				return model;
			}
		}

		public object LiveById(int[] id)
		{
			return id.ToDictionary(x => x, x => CacheWorker.Read(x));
		}

		public object History(string[] tags, DateTime old, DateTime young, int resolution)
		{
			using (var db = new DatabaseContext())
			{
				var model = db.ReadHistory(tags, old, young, resolution);

				return model;
			}
		}

		public object FlatHistory(string[] tags, bool live, DateTime old, DateTime young, int resolution)
		{
			using (var db = new DatabaseContext())
			{
				var model = live
					? db.ReadLive(tags)
					: db.ReadHistory(tags, old, young, resolution);

				var flat = new List<FlatTagValue>();
				int id = 0;
					
				foreach (var range in model)
				{
					foreach (var value in range.Values)
					{
						flat.Add(new FlatTagValue
						{
							Id = id++,
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
