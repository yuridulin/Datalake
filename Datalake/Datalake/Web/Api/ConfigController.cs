using Datalake.Database;
using Datalake.Web.Models;
using System;
using System.Linq;

namespace Datalake.Web.Api
{
	public class ConfigController : Controller
	{
		public object LastUpdate()
		{
			using (var db = new DatabaseContext())
			{
				return db.GetUpdateDate();
			}
		}

		public object Statistic(DateTime last)
		{
			using (var db = new DatabaseContext())
			{
				var TotalTagsCount = db.Tags.Count();
				var TotalSourcesCount = db.Sources.Count();
				var WritesInMinute = db.TagsLive.ToList().Where(x => (DateTime.Now - x.Date) < TimeSpan.FromMinutes(1)).Count();

				var Logs = db.ProgramLog.Where(x => x.Timestamp > last).ToList();
				if (Logs.Any()) { last = Logs.OrderByDescending(x => x.Timestamp).Select(x => x.Timestamp).First(); }

				return new { TotalTagsCount, TotalSourcesCount, WritesInMinute, Logs, Last = last };
			}
		}
	}
}
