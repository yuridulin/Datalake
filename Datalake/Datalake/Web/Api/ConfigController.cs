using Datalake.Database;
using Datalake.Web.Models;
using System;
using System.Linq;

namespace Datalake.Web.Api
{
	public class ConfigController : Controller
	{
		public Result LastUpdate()
		{
			return Data(Cache.LastUpdate);
		}

		public Result Statistic(DateTime last)
		{
			using (var db = new DatabaseContext())
			{
				var TotalTagsCount = db.Tags.Count();
				var TotalSourcesCount = db.Sources.Count();
				var WritesInMinute = Cache.Live.Values.ToList().Where(x => (DateTime.Now - x.Date) < TimeSpan.FromMinutes(1)).Count();

				return Data(new { TotalTagsCount, TotalSourcesCount, WritesInMinute, Logs = new string[0], Last = last });
			}
		}
	}
}
