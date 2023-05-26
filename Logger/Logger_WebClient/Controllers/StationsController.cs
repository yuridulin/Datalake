using LinqToDB;
using Logger.Database;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Logger.WebClient.Controllers
{
	public class StationsController : Controller
	{
		public ActionResult Index() => View();

		public ActionResult Online() => View();

		public ActionResult Create() => View();

		public ActionResult Details(string Id) => View(model: Id);

		public ActionResult Edit(string Id) => View(model: Id);


		public ActionResult DoCreate(Station station)
		{
			try
			{
				using (var db = new DatabaseContext())
				{
					db.Stations
						.Value(x => x.Endpoint, station.Endpoint.ToUpper())
						.Value(x => x.Description, "")
						.Value(x => x.StationConfigId, 0)
						.Value(x => x.LastTimeAlive, DateTime.MinValue)
						.Value(x => x.DeployTime, DateTime.MinValue)
						.Value(x => x.DeployStatus, StationDeployState.No_Info)
						.Insert();

					db.Settings
						.Set(x => x.LastUpdate, DateTime.Now)
						.Update();

					return Json(new
					{
						Done = "Станция добавлена",
						Link = Url.Action("", "stations"),
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

		public ActionResult DoEdit(Station station)
		{
			try
			{
				using (var db = new DatabaseContext())
				{
					db.Stations
						.Where(x => x.Endpoint == station.Endpoint)
						.Set(x => x.Description, station.Description)
						.Set(x => x.StationConfigId, station.StationConfigId)
						.Update();

					db.Settings
						.Set(x => x.LastUpdate, DateTime.Now)
						.Update();

					return Json(new
					{
						Done = "Станция сохранена",
						Link = Url.Action("details", "stations", new { Id = station.Endpoint }),
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

		public ActionResult DoDelete(Station station)
		{
			try
			{
				using (var db = new DatabaseContext())
				{
					db.Stations
						.Where(x => x.Endpoint == station.Endpoint.ToUpper())
						.Delete();

					db.Settings
						.Set(x => x.LastUpdate, DateTime.Now)
						.Update();

					return Json(new
					{
						Done = "Станция удалена",
						Link = Url.Action("", "stations"),
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

		public ActionResult Install(string Id)
		{
			try
			{
				using (var db = new DatabaseContext())
				{
					db.Stations
						.Where(x => x.Endpoint == Id)
						.Set(x => x.DeployStatus, StationDeployState.WaitForInstall)
						.Update();
				}

				return Json(new
				{
					Done = "Назначена установка агента на станцию",
					UpdateView = true,
				});
			}
			catch (Exception e)
			{
				return Json(new
				{
					Error = e.Message
				});
			}
		}

		public ActionResult UnInstall(string Id)
		{
			try
			{
				using (var db = new DatabaseContext())
				{
					db.Stations
						.Where(x => x.Endpoint == Id)
						.Set(x => x.DeployStatus, StationDeployState.WaitForUninstall)
						.Update();
				}

				return Json(new
				{
					Done = "Назначено удаление агента со станции",
					UpdateView = true,
				});
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