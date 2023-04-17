using LinqToDB;
using Logger.Database;
using Logger.Web.Models;
using System.Linq;

namespace Logger.Web.Api
{
	public class PresetsController : Controller
	{
		public object List()
		{
			using (var db = new DatabaseContext())
			{
				return db.Presets
					.ToList();
			}
		}

		public object Create(string name, string description)
		{
			using (var db = new DatabaseContext())
			{
				db.Presets
					.Value(x => x.Name, name)
					.Value(x => x.Description, description)
					.Insert();

				return new { Done = "Назначение успешно добавлено" };
			}
		}

		public object Read(int id)
		{
			using (var db = new DatabaseContext())
			{
				var model = db.Presets
					.FirstOrDefault(x => x.Id == id);

				if (model == null) return new { Error = "Назначение не найдено" };

				return model;
			}
		}

		public object Update(int id, string name, string description)
		{
			using (var db = new DatabaseContext())
			{
				db.Presets
					.Where(x => x.Id == id)
					.Set(x => x.Name, name)
					.Set(x => x.Description, description)
					.Update();

				return new { Done = "Назначение успешно сохранено" };
			}
		}

		public object Delete(int id)
		{
			using (var db = new DatabaseContext())
			{
				db.Presets
					.Where(x => x.Id == id)
					.Delete();

				return new { Done = "Назначение успешно удалено" };
			}
		}
	}
}
