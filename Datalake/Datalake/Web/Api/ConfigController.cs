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
			return Cache.LastUpdate;
		}

		public object Statistic(DateTime last)
		{
			using (var db = new DatabaseContext())
			{
				var TotalTagsCount = db.Tags.Count();
				var TotalSourcesCount = db.Sources.Count();
				var WritesInMinute = Cache.Live.Values.Where(x => (DateTime.Now - x.Date) < TimeSpan.FromMinutes(1)).Count();

				return new { TotalTagsCount, TotalSourcesCount, WritesInMinute, Logs = new string[0], Last = last };
			}
		}
	}
}
