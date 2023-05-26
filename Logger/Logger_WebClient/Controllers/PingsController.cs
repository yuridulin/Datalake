using LinqToDB;
using Logger.Database;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Logger.WebClient.Controllers
{
	public class PingsController : Controller
	{
		public ActionResult Index() => View();

		public ActionResult Create() => View();

		public ActionResult Details(int Id) => View(model: Id);

		public ActionResult Edit(int Id) => View(model: Id);


		public ActionResult DoCreate(string Name, string Target, int Interval, string Template, int Value)
		{
			try
			{
				using (var db = new DatabaseContext())
				{
					db.ActionsPings
						.Value(x => x.Name, Name)
						.Value(x => x.Target, Target)
						.Value(x => x.Interval, Interval)
						.Value(x => x.Template, Template)
						.Value(x => x.Value, Value)
						.Insert();

					db.Settings
						.Set(x => x.LastUpdate, DateTime.Now)
						.Update();

					return Json(new
					{
						Done = "Ping проверка создана",
						Link = Url.Action("", "pings"),
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

		public ActionResult DoEdit(int Id, string Name, string Target, int Interval, string Template, int Value)
		{
			try
			{
				using (var db = new DatabaseContext())
				{
					db.ActionsPings
						.Where(x => x.Id == Id)
						.Set(x => x.Name, Name)
						.Set(x => x.Target, Target)
						.Set(x => x.Interval, Interval)
						.Set(x => x.Template, Template)
						.Set(x => x.Value, Value)
						.Update();

					db.Settings
						.Set(x => x.LastUpdate, DateTime.Now)
						.Update();

					return Json(new
					{
						Done = "Ping проверка сохранена",
						Link = Url.Action("", "pings"),
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
					db.ActionsPings
						.Where(x => x.Id == Id)
						.Delete();

					db.Rel_StationConfig_ActionPing
						.Where(x => x.PingActionId == Id)
						.Delete();

					db.Settings
						.Set(x => x.LastUpdate, DateTime.Now)
						.Update();

					return Json(new
					{
						Done = "Ping проверка удалена",
						Link = Url.Action("", "pings"),
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