using Datalake.Collector.Models;
using Datalake.Database;
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
			string address;

			using (var db = new DatabaseContext())
			{
				var source = db.Sources.FirstOrDefault(x => x.Id == id);
				if (source == null) return new { Error = "Источник не найден." };
				address = source.Address;
			}

			var res = SourcePacket.AskInopc(new string[0], address);

			return res.Tags
				.Select(x => x.Name)
				.OrderBy(x => x)
				.ToList();
		}

		public object Create(string name, string address)
		{
			using (var db = new DatabaseContext())
			{
				if (db.Sources.Any(x => x.Name == name)) return new { Error = "Уже существует источник с таким именем." };

				db.Sources
					.Value(x => x.Name, name)
					.Value(x => x.Address, address)
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

		public object Update(int id, string name, string address)
		{
			using (var db = new DatabaseContext())
			{
				if (!db.Sources.Any(x => x.Id == id)) return new { Error = "Источник не найден." };
				if (db.Sources.Any(x => x.Name == name)) return new { Error = "Уже существует источник с таким именем." };

				db.Sources
					.Where(x => x.Id == id)
					.Set(x => x.Name, name)
					.Set(x => x.Address, address)
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
