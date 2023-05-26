using LinqToDB;
using Logger.Database;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Logger.WebClient.Controllers
{
	public class SqlController : Controller
	{
		public ActionResult Index() => View();

		public ActionResult Create() => View();

		public ActionResult Details(int Id) => View(model: Id);

		public ActionResult Edit(int Id) => View(model: Id);


		public ActionResult DoCreate(ActionSql action)
		{
			try
			{
				using (var db = new DatabaseContext())
				{
					db.ActionsSql
						.Value(x => x.CommandCode, action.CommandCode)
						.Value(x => x.CommandTimeout, action.CommandTimeout)
						.Value(x => x.ConnectionString, action.ConnectionString)
						.Value(x => x.DatabaseType, action.DatabaseType)
						.Value(x => x.Description, action.Description)
						.Value(x => x.Interval, action.Interval)
						.Value(x => x.Name, action.Name)
						.Insert();

					db.Settings
						.Set(x => x.LastUpdate, DateTime.Now)
						.Update();

					return Json(new
					{
						Done = "Sql процедура создана",
						Link = Url.Action("", "sql"),
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

		public ActionResult DoEdit(
			int Id,
			string CommandCode,
			int CommandTimeout,
			string ConnectionString,
			string DatabaseType,
			string Description,
			string Name,
			int Interval,
			string ComparersJson
		)
		{
			try
			{
				using (var db = new DatabaseContext())
				{
					db.ActionsSql
						.Where(x => x.Id == Id)
						.Set(x => x.Name, Name)
						.Set(x => x.Description, Description)
						.Set(x => x.DatabaseType, DatabaseType)
						.Set(x => x.ConnectionString, ConnectionString)
						.Set(x => x.CommandTimeout, CommandTimeout)
						.Set(x => x.CommandCode, CommandCode)
						.Set(x => x.Interval, Interval)
						.Set(x => x.ComparersJson, ComparersJson)
						.Update();

					db.Settings
						.Set(x => x.LastUpdate, DateTime.Now)
						.Update();

					return Json(new
					{
						Done = "Sql процедура сохранена",
						Link = Url.Action("", "sql"),
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
					db.ActionsSql
						.Where(x => x.Id == Id)
						.Delete();

					db.Rel_StationConfig_ActionSql
						.Where(x => x.ActionSqlId == Id)
						.Delete();

					db.Settings
						.Set(x => x.LastUpdate, DateTime.Now)
						.Update();

					return Json(new
					{
						Done = "Sql процедура удалена",
						Link = Url.Action("", "sql"),
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