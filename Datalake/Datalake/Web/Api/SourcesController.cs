using Datalake.Database;
using Datalake.Database.Enums;
using Datalake.Web.Models;
using LinqToDB;
using System.Linq;

namespace Datalake.Web.Api
{
	public class SourcesController : Controller
	{
		public object List()
		{
			using (var db = new DatabaseContext())
			{
				return db.Sources
					.OrderBy(x => x.Name)
					.ToList();
			}
		}

		public object Items(int id)
		{
			using (var db = new DatabaseContext())
			{
				var source = db.Sources.FirstOrDefault(x => x.Id == id);
				if (source == null) return new { Error = "Источник не найден." };

				var res = source.GetItems();

				return res.Tags.Select(x => x.Name).ToList();
			}
		}

		public object Tags(int id)
		{
			using (var db = new DatabaseContext())
			{
				var source = db.Sources.FirstOrDefault(x => x.Id == id);
				if (source == null) return new { Error = "Источник не найден." };

				var res = source.GetItems();
				var items = res.Tags.Select(x => new { x.Name, x.Type }).ToList();

				var tags = db.Tags
					.Where(x => x.SourceId == id)
					.Where(x => items.Select(y => y.Name).Contains(x.SourceItem))
					.ToList();

				return items.Select(x => new
				{
					Item = x.Name,
					Type = (int)x.Type,
					Tag = tags.FirstOrDefault(t => t.SourceItem == x.Name)
				});
			}
		}

		public object Create()
		{
			using (var db = new DatabaseContext())
			{
				db.Sources
					.Value(x => x.Name, "Новый источник данных")
					.Value(x => x.Address, string.Empty)
					.Value(x => x.Type, SourceType.Inopc)
					.Insert();

				return new { Done = "Источник успешно добавлен." };
			}
		}

		public object Read(int id)
		{
			using (var db = new DatabaseContext())
			{
				var source = db.Sources.FirstOrDefault(x => x.Id == id);

				if (source == null) return new { Error = "Источник не найден." };

				return source;
			}
		}

		public object Update(int id, string name, string address, int type)
		{
			using (var db = new DatabaseContext())
			{
				if (!db.Sources.Any(x => x.Id == id)) return new { Error = "Источник не найден." };
				if (db.Sources.Where(x => x.Id != id).Any(x => x.Name == name)) return new { Error = "Уже существует источник с таким именем." };

				db.Sources
					.Where(x => x.Id == id)
					.Set(x => x.Name, name)
					.Set(x => x.Address, address)
					.Set(x => x.Type, (SourceType)type)
					.Update();

				return new { Done = "Источник успешно сохранён." };
			}
		}

		public object Delete(int id)
		{
			using (var db = new DatabaseContext())
			{
				db.Sources
					.Where(x => x.Id == id)
					.Delete();

				return new { Done = "Источник успешно удалён." };
			}
		}
	}
}
