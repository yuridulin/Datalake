using Datalake.Database;
using Datalake.Database.Enums;
using Datalake.Web.Models;
using LinqToDB;
using System;
using System.Linq;

namespace Datalake.Web.Api
{
	public class TagsController : Controller
	{
		public object List()
		{
			using (var db = new DatabaseContext())
			{
				var sources = db.Sources.ToList();

				return db.Tags
					.Select(x => new
					{
						x.Id,
						x.Name,
						x.Type,
						x.Description,
						x.SourceId,
						x.SourceItem,
						x.Interval,
						Source = sources.DefaultIfEmpty(new Source { Name = "?" }).FirstOrDefault(s => s.Id == x.SourceId).Name,
					})
					.OrderBy(x => x.Name)
					.ToList();
			}
		}

		public object Create(string tagName)
		{
			using (var db = new DatabaseContext())
			{
				if (db.Tags.Any(x => x.Name == tagName)) return new { Error = "Уже существует такой тег. Введите другое имя тега." };

				var id = db.Tags
					.Value(x => x.Name, tagName)
					.Value(x => x.Description, string.Empty)
					.Value(x => x.SourceId, 0)
					.Value(x => x.SourceItem, string.Empty)
					.Value(x => x.Interval, 0)
					.Value(x => x.Type, TagType.String)
					.Value(x => x.IsScaling, false)
					.Value(x => x.MinRaw, 0)
					.Value(x => x.MaxRaw, 0)
					.Value(x => x.MinEU, 0)
					.Value(x => x.MaxEU, 0)
					.InsertWithInt32Identity();

				if (id.HasValue)
				{
					db.TagsLive
						.Value(x => x.TagId, id.Value)
						.Value(x => x.Date, DateTime.Now)
						.Value(x => x.Quality, TagQuality.Bad)
						.Insert();

					db.SetUpdateDate();

					return new { Done = "Тег успешно добавлен." };
				}
				else
				{
					return new { Done = "Ошибка при добавлении тега." };
				}
			}
		}

		public object Read(int id)
		{
			using (var db = new DatabaseContext())
			{
				var tag = db.Tags.FirstOrDefault(x => x.Id == id);

				if (tag == null) return new { Error = "Тег не найден." };

				return tag;
			}
		}

		public object Update(int id, string tagName, string description, int sourceId, string sourceItem, short interval, byte tagType)
		{
			using (var db = new DatabaseContext())
			{
				if (!db.Tags.Any(x => x.Name == tagName)) return new { Error = "Тег не найден." };

				db.Tags
					.Where(x => x.Id == id)
					.Set(x => x.Name, tagName)
					.Set(x => x.Description, description)
					.Set(x => x.SourceId, sourceId)
					.Set(x => x.SourceItem, sourceItem)
					.Set(x => x.Interval, interval)
					.Set(x => x.Type, (TagType)tagType)
					.Update();

				db.SetUpdateDate();

				return new { Done = "Тег успешно сохранён." };
			}
		}

		public object Delete(int id)
		{
			using (var db = new DatabaseContext())
			{
				db.Tags
					.Where(x => x.Id == id)
					.Delete();

				db.TagsLive
					.Where(x => x.TagId == id)
					.Delete();

				db.SetUpdateDate();

				return new { Done = "Тег успешно удалён." };
			}
		}
	}
}
