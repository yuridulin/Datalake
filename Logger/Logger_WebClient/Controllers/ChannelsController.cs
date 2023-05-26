using LinqToDB;
using Logger.Database;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Logger.WebClient.Controllers
{
	public class ChannelsController : Controller
	{
		public ActionResult New() => View();

		public ActionResult Details(int Id) => View(model: Id);

		public ActionResult Edit(int Id) => View(model: Id);


		[HttpPost]
		public ActionResult Create(Channel channel)
		{
			try
			{
				using (var db = new DatabaseContext())
				{
					int? id = db.Channels
						.Value(x => x.Name, channel.Name)
						.Value(x => x.Type, channel.Type)
						.InsertWithInt32Identity();

					if (!id.HasValue) throw new Exception("Новый идентификатор не получен");

					channel.Id = id.Value;
				}

				return Json(new
				{
					Done = "Канал сохранён",
					Link = Url.Action("Details", "Channels", new { channel.Id }),
					UpdateTree = true
				});
			}
			catch (Exception e)
			{
				return Json(new { Error = e.Message });
			}
		}

		[HttpPost]
		public ActionResult Update(Channel channel)
		{
			try
			{
				using (var db = new DatabaseContext())
				{
					db.Channels
						.Where(x => x.Id == channel.Id)
						.Set(x => x.Name, channel.Name)
						.Set(x => x.Type, channel.Type)
						.Update();
				}

				return Json(new 
				{
					Done = "Канал сохранён",
					UpdateView = true,
					UpdateTree = true
				});
			}
			catch (Exception e)
			{
				return Json(new { Error = e.Message });
			}
		}

		[HttpPost]
		public ActionResult Delete(int Id)
		{
			try
			{
				using (var db = new DatabaseContext())
				{
					db.Channels
						.Where(x => x.Id == Id)
						.Delete();
				}

				return Json(new 
				{
					Done = "Канал удалён",
					Link = "#/",
					UpdateTree = true
				});
			}
			catch (Exception e)
			{
				return Json(new { Error = e.Message });
			}
		}
	}
}