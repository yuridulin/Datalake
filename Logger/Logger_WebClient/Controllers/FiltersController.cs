using LinqToDB;
using LinqToDB.Data;
using Logger.Database;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Logger.WebClient.Controllers
{
	public class FiltersController : Controller
	{
		public ActionResult Index() => View();

		public ActionResult Create() => View();

		public ActionResult Details(int Id) => View(model: Id);

		public ActionResult Edit(int Id) => View(model: Id);


		public ActionResult DoCreate(LogFilter filter)
		{
			try
			{
				using (var db = new DatabaseContext())
				{
					db.LogsFilters
						.Value(x => x.Allow, filter.Allow)
						.Value(x => x.Categories, filter.Categories ?? "")
						.Value(x => x.Description, filter.Description ?? "")
						.Value(x => x.Endpoints, filter.Endpoints ?? "")
						.Value(x => x.EventIds, filter.EventIds ?? "")
						.Value(x => x.Journals, filter.Journals ?? "")
						.Value(x => x.Name, filter.Name ?? "")
						.Value(x => x.Sources, filter.Sources ?? "")
						.Value(x => x.Types, filter.Types ?? "")
						.Insert();

					db.Settings
						.Set(x => x.LastUpdate, DateTime.Now)
						.Update();

					return Json(new
					{
						Done = "Фильтр добавлен",
						Link = Url.Action("", "filters")
					});
				}
			}
			catch (Exception e)
			{
				return Json(new
				{
					Error = e.Message
				});
			}
		}

		public ActionResult DoEdit(LogFilter filter, int[] channels)
		{
			try
			{
				using (var db = new DatabaseContext())
				{
					db.LogsFilters
						.Where(x => x.Id == filter.Id)
						.Set(x => x.Allow, filter.Allow)
						.Set(x => x.Categories, filter.Categories ?? "")
						.Set(x => x.Description, filter.Description ?? "")
						.Set(x => x.Endpoints, filter.Endpoints ?? "")
						.Set(x => x.EventIds, filter.EventIds ?? "")
						.Set(x => x.Journals, filter.Journals ?? "")
						.Set(x => x.Name, filter.Name ?? "")
						.Set(x => x.Sources, filter.Sources ?? "")
						.Set(x => x.Types, filter.Types ?? "")
						.Update();

					db.Rel_LogFilter_Channel
						.Where(x => x.LogFilterId == filter.Id)
						.Delete();

					db.BulkCopy(channels
						.Where(x => x != 0)
						.Select(x => new Rel_LogFilter_Channel 
						{
							LogFilterId = filter.Id,
							ChannelId = x
						})
						.ToList());

					db.Settings
						.Set(x => x.LastUpdate, DateTime.Now)
						.Update();

					return Json(new
					{
						Done = "Фильтр сохранен",
						Link = Url.Action("", "filters")
					});
				}
			}
			catch (Exception e)
			{
				return Json(new
				{
					Error = e.Message
				});
			}
		}

		public ActionResult DoDelete(int Id)
		{
			try
			{
				using (var db = new DatabaseContext())
				{
					db.LogsFilters
						.Where(x => x.Id == Id)
						.Delete();

					db.Settings
						.Set(x => x.LastUpdate, DateTime.Now)
						.Update();

					return Json(new
					{
						Done = "Фильтр удален",
						Link = Url.Action("", "filters")
					});
				}
			}
			catch (Exception e)
			{
				return Json(new
				{
					Error = e.Message
				});
			}
		}
	}
}