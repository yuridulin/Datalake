using Datalake.Database;
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
						x.TagName,
						x.TagType,
						x.Description,
						x.SourceId,
						x.SourceItem,
						x.Interval,
						Source = sources.DefaultIfEmpty(new Source { Name = "?" }).FirstOrDefault(s => s.Id == x.SourceId).Name,

					})
					.ToList();
			}
		}

		public object Create(string tagName, string description, int sourceId, string sourceItem, short interval, byte tagType)
		{
			using (var db = new DatabaseContext())
			{
				if (db.Tags.Any(x => x.TagName == tagName)) return new { Error = "Уже существует такой тег. Введите другое имя тега." };

				db.Tags
					.Value(x => x.TagName, tagName)
					.Value(x => x.Description, description)
					.Value(x => x.SourceId, sourceId)
					.Value(x => x.SourceItem, sourceItem)
					.Value(x => x.Interval, interval)
					.Value(x => x.TagType, (TagType)tagType)
					.Insert();

				db.TagsLive
					.Value(x => x.TagName, tagName)
					.Value(x => x.Date, DateTime.Now)
					.Value(x => x.Quality, (short)0)
					.Insert();

				db.SetUpdateDate();

				return new { Done = "Тег успешно добавлен" };
			}
		}

		public object Read(string tagName)
		{
			using (var db = new DatabaseContext())
			{
				var tag = db.Tags.FirstOrDefault(x => x.TagName == tagName);

				if (tag == null) return new { Error = "Тег с таким именем не найден." };

				return tag;
			}
		}

		public object Update(string tagName, string description, int sourceId, short interval, byte tagType)
		{
			using (var db = new DatabaseContext())
			{
				if (!db.Tags.Any(x => x.TagName == tagName)) return new { Error = "Тег с таким именем не найден." };

				db.Tags
					.Where(x => x.TagName == tagName)
					.Set(x => x.Description, description)
					.Set(x => x.SourceId, sourceId)
					.Set(x => x.Interval, interval)
					.Set(x => x.TagType, (TagType)tagType)
					.Update();

				db.SetUpdateDate();

				return new { Done = "Тег успешно сохранён." };
			}
		}

		public object Delete(string tagName)
		{
			using (var db = new DatabaseContext())
			{
				db.Tags
					.Where(x => x.TagName == tagName)
					.Delete();

				db.TagsLive
					.Where(x => x.TagName == tagName)
					.Delete();

				db.SetUpdateDate();

				return new { Done = "Тег успешно удалён." };
			}
		}
	}
}
