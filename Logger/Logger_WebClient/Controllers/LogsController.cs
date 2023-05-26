using LinqToDB;
using LinqToDB.Data;
using Logger.Database;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Logger.WebClient.Controllers
{
	public class LogsController : Controller
	{
		public ActionResult Index() => View();

		public ActionResult Details(int Id) => View(model: Id);

		public ActionResult SetAsViewed(int Id)
		{
			try
			{
				using (var db = new DatabaseContext())
				{
					var existReaction = db.LogsReactions
						.Where(x => x.LogId == Id)
						.Where(x => x.Username == User.Identity.Name)
						.FirstOrDefault();

					if (existReaction == null)
					{
						db.LogsReactions
							.Value(x => x.Date, DateTime.Now)
							.Value(x => x.Username, User.Identity.Name)
							.Value(x => x.LogId, Id)
							.Insert();
					}

					return Json(new
					{
						Link = Url.Action("", "logs")
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

		public ActionResult SetSeenAsViewed(int[] LogsId)
		{
			try
			{
				using (var db = new DatabaseContext())
				{
					var existReactions = db.LogsReactions
						.Where(x => LogsId.Contains(x.LogId))
						.Where(x => x.Username == User.Identity.Name)
						.Select(x => x.LogId)
						.ToList();

					var newReactions = LogsId
						.Where(x => !existReactions.Contains(x))
						.Select(x => new LogReaction
						{
							Date = DateTime.Now,
							Username = User.Identity.Name,
							LogId = x
						})
						.ToList();

					db.BulkCopy(newReactions);

					return Json(new
					{
						UpdateView = true
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

		public ActionResult SetAllAsViewed()
		{
			try
			{
				using (var db = new DatabaseContext())
				{
					var query = from log in db.Logs
								from web in db.Rel_Log_WebView.InnerJoin(x => x.LogId == log.Id)
								from r in db.LogsReactions.LeftJoin(x => x.LogId == log.Id && x.Username == User.Identity.Name)
								where r == null
								select new LogReaction
								{
									Date = DateTime.Now,
									LogId = log.Id,
									Username = User.Identity.Name
								};

					var reactions = query.ToList();

					db.BulkCopy(reactions);

					return Json(new
					{
						UpdateView = true
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