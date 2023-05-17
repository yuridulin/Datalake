using LinqToDB;
using Logger.Database;
using Logger.Web.Models;
using System.Linq;

namespace Logger.Web.Api
{
	public class FiltersController : Controller
	{
		public object List()
		{
			using (var db = new DatabaseContext())
			{
				return db.Filters
					.ToList();
			}
		}

		public object Create(string name, string description, bool isAllowing, string[] categories, string[] computers, int[] eventIds, string[] journals, string[] sources, int[] types, string[] usernames)
		{
			using (var db = new DatabaseContext())
			{
				db.Filters
					.Value(x => x.Name, name)
					.Value(x => x.Description, description)
					.Value(x => x.IsAllowing, isAllowing)
					.Value(x => x.CategoriesArray, categories)
					.Value(x => x.ComputersArray, computers)
					.Value(x => x.EventIdsArray, eventIds)
					.Value(x => x.JournalsArray, journals)
					.Value(x => x.SourcesArray, sources)
					.Value(x => x.TypesArray, types)
					.Value(x => x.UsernamesArray, usernames)
					.Insert();
			}

			return new { Done = "Фильтр успешно создан" };
		}

		public object Read(int id)
		{
			using (var db = new DatabaseContext())
			{
				var model = db.Filters.FirstOrDefault(x => x.Id == id);
				if (model == null) return new { Error = "Фильтр не найден" };

				return model;
			}
		}

		public object Update(int id, string name, string description, bool isAllowing, string[] categories, string[] computers, int[] eventIds, string[] journals, string[] sources, int[] types, string[] usernames)
		{
			using (var db =new DatabaseContext())
			{
				db.Filters
					.Where(x => x.Id == id)
					.Set(x => x.Name, name)
					.Set(x => x.Description, description)
					.Set(x => x.IsAllowing, isAllowing)
					.Set(x => x.CategoriesArray, categories)
					.Set(x => x.ComputersArray, computers)
					.Set(x => x.EventIdsArray, eventIds)
					.Set(x => x.JournalsArray, journals)
					.Set(x => x.SourcesArray, sources)
					.Set(x => x.TypesArray, types)
					.Set(x => x.UsernamesArray, usernames)
					.Update();
			}

			return new { Done = "Фильтр успешно сохранён" };
		}

		public object Delete(int id)
		{
			using (var db = new DatabaseContext())
			{
				db.Filters
					.Where(x => x.Id == id)
					.Delete();
			}

			return new { Done = "Фильтр успешно удалён" };
		}
	}
}
