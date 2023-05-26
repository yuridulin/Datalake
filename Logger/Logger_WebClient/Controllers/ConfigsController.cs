using LinqToDB;
using LinqToDB.Data;
using Logger.Database;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Logger.WebClient.Controllers
{
	public class ConfigsController : Controller
	{
		public ActionResult Index() => View();

		public ActionResult Create() => View();

		public ActionResult Details(int Id) => View(model: Id);

		public ActionResult Edit(int Id) => View(model: Id);


		public ActionResult DoCreate(StationConfig config, int[] Pings, int[] Sql, int[] Filters)
		{
			try
			{
				using (var db = new DatabaseContext())
				{
					int? id = db.StationsConfigs
						.Value(x => x.Name, config.Name)
						.Value(x => x.Description, config.Description)
						.InsertWithInt32Identity();

					if (!id.HasValue) throw new Exception("Не получен идентификатор новой конфигурации");

					db.BulkCopy(Pings.Where(x => x != 0).Select(x => new Rel_StationConfig_ActionPing { PingActionId = x, StationConfigId = id.Value }).ToList());
					db.BulkCopy(Sql.Where(x => x != 0).Select(x => new Rel_StationConfig_ActionSql { ActionSqlId = x, StationConfigId = id.Value }).ToList());
					db.BulkCopy(Filters.Where(x => x != 0).Select(x => new Rel_StationConfig_LogFilter { LogFilterId = x, StationConfigId = id.Value }).ToList());

					db.Settings
						.Set(x => x.LastUpdate, DateTime.Now)
						.Update();

					return Json(new
					{
						Done = "Конфигурация создана",
						Link = Url.Action("", "configs"),
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

		public ActionResult DoEdit(StationConfig config, int[] Pings, int[] Sql, int[] Filters)
		{
			try
			{
				using (var db = new DatabaseContext())
				{
					db.StationsConfigs
						.Where(x => x.Id == config.Id)
						.Set(x => x.Name, config.Name)
						.Set(x => x.Description, config.Description)
						.Update();

					db.Rel_StationConfig_ActionPing
						.Where(x => x.StationConfigId == config.Id)
						.Delete();
					db.BulkCopy(Pings.Where(x => x != 0).Select(x => new Rel_StationConfig_ActionPing { PingActionId = x, StationConfigId = config.Id }).ToList());

					db.Rel_StationConfig_ActionSql
						.Where(x => x.StationConfigId == config.Id)
						.Delete();
					db.BulkCopy(Sql.Where(x => x != 0).Select(x => new Rel_StationConfig_ActionSql { ActionSqlId = x, StationConfigId = config.Id }).ToList());

					db.Rel_StationConfig_LogFilter
						.Where(x => x.StationConfigId == config.Id)
						.Delete();
					db.BulkCopy(Filters.Where(x => x != 0).Select(x => new Rel_StationConfig_LogFilter { LogFilterId = x, StationConfigId = config.Id }).ToList());

					db.Settings
						.Set(x => x.LastUpdate, DateTime.Now)
						.Update();

					return Json(new
					{
						Done = "Конфигурация сохранена",
						Link = Url.Action("", "configs"),
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
					db.StationsConfigs
						.Where(x => x.Id == Id)
						.Delete();

					db.Rel_StationConfig_ActionPing
						.Where(x => x.StationConfigId == Id)
						.Delete();

					db.Rel_StationConfig_ActionSql
						.Where(x => x.StationConfigId == Id)
						.Delete();

					db.Rel_StationConfig_LogFilter
						.Where(x => x.StationConfigId == Id)
						.Delete();

					db.Settings
						.Set(x => x.LastUpdate, DateTime.Now)
						.Update();

					return Json(new
					{
						Done = "Конфигурация удалена",
						Link = Url.Action("", "configs"),
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