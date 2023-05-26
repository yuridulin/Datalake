using LinqToDB;
using Logger.Database;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Logger.WebClient.Controllers
{
	public class WatchersController : Controller
	{
		public async Task<object> Details(ActionWatcher watcher)
		{
			using (var db = new DatabaseContext())
			{
				var model = await db.ActionsWatchers
					.FirstOrDefaultAsync(x => x.Id == watcher.Id);

				return model;
			}
		}

		public async Task<object> Create(ActionWatcher watcher)
		{
			using (var db = new DatabaseContext())
			{
				await db.ActionsWatchers
					.Value(x => x.Name, watcher.Name)
					.Value(x => x.WatcherId, watcher.WatcherId)
					.Value(x => x.Template, watcher.Template)
					.Value(x => x.Comparer, watcher.Comparer)
					.Value(x => x.Value, watcher.Value)
					.InsertAsync();

				await db.Settings
					.Set(x => x.LastUpdate, DateTime.Now)
					.UpdateAsync();

				return new { Done = true };
			}
		}

		public async Task<object> Update(ActionWatcher watcher)
		{
			using (var db = new DatabaseContext())
			{
				await db.ActionsWatchers
					.Set(x => x.Name, watcher.Name)
					.Set(x => x.WatcherId, watcher.WatcherId)
					.Set(x => x.Template, watcher.Template)
					.Set(x => x.Comparer, watcher.Comparer)
					.Set(x => x.Value, watcher.Value)
					.UpdateAsync();

				await db.Settings
					.Set(x => x.LastUpdate, DateTime.Now)
					.UpdateAsync();

				return new { Done = true };
			}
		}

		public async Task<object> Delete(ActionWatcher watcher)
		{
			using (var db = new DatabaseContext())
			{
				await db.ActionsWatchers
					.Where(x => x.Id == watcher.Id)
					.DeleteAsync();

				await db.Rel_StationConfig_ActionWatcher
					.Where(x => x.WatcherActionId == watcher.Id)
					.DeleteAsync();

				await db.Settings
					.Set(x => x.LastUpdate, DateTime.Now)
					.UpdateAsync();

				return new { Done = true };
			}
		}
	}
}