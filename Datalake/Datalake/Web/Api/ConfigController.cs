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

		public object Statistic()
		{
			using (var db = new DatabaseContext())
			{
				var TotalTagsCount = db.Tags.Count();
				var TotalSourcesCount = db.Sources.Count();
				var WritesInMinute = db.TagsLive.ToList().Where(x => (DateTime.Now - x.Date) < TimeSpan.FromMinutes(1)).Count();

				return new { TotalTagsCount, TotalSourcesCount, WritesInMinute };
			}
		}
	}
}
