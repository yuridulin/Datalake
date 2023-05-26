using Datalake.Collector;
using Datalake.Database;
using Datalake.Web.Models;
using System;

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

		public object History(string[] tags, DateTime old, DateTime young, int resolution)
		{
			using (var db = new DatabaseContext())
			{
				var model = db.ReadHistory(tags, old, young, resolution);

				return model;
			}
		}
	}
}
