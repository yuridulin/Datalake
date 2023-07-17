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

		public object Types()
		{
			return Enum.GetValues(typeof(TagType)).Cast<TagType>().ToDictionary(x => (int)x, x => x.ToString());
		}

		public object Inputs(int id)
		{
			using (var db = new DatabaseContext())
			{
				var outputs = db.Rel_Tag_Input
					.Where(x => x.InputTagId == id)
					.Select(x => x.TagId)
					.ToList();

				return db.Tags
					.Where(x => x.Id != id)
					.Where(x => !outputs.Contains(x.Id))
					.Select(x => new { x.Id, x.Name })
					.ToList();
			}
		}

		public object Create()
		{
			using (var db = new DatabaseContext())
			{
				string name = db.Tags.OrderByDescending(x => x.Name).Select(x => x.Name).FirstOrDefault() ?? "New_tag_1";
				int number = int.TryParse(name.Replace("New_tag_", ""), out int i) ? i : 1;
				name = "New_tag_" + number;

				var tag = new Tag { Name = name };

				var id = db.InsertWithInt32Identity(tag);

				db.TagsLive
					.Value(x => x.TagId, id)
					.Value(x => x.Date, DateTime.Now)
					.Value(x => x.Quality, TagQuality.Bad)
					.Insert();

				db.SetUpdateDate();

				return Done("Тег успешно добавлен.");
			}
		}

		public object CreateFromSource(int sourceId, string sourceItem)
		{
			using (var db = new DatabaseContext())
			{
				var name = sourceItem.Replace('.', '_');
				bool created = false;
				int i = 1;

				do
				{
					if (db.Tags.Any(x => x.Name == name))
					{
						name = sourceItem + "_" + i++;
					}
					else
					{
						var tag = new Tag
						{
							Name = name,
							SourceId = sourceId,
							SourceItem = sourceItem,
						};

						var id = db.InsertWithInt32Identity(tag);

						db.TagsLive
							.Value(x => x.TagId, id)
							.Value(x => x.Date, DateTime.Now)
							.Value(x => x.Quality, TagQuality.Bad)
							.Insert();

						created = true;
					}
				}
				while (!created);

				db.SetUpdateDate();
			}

			return new { Done = true };
		}

		public object Read(int id)
		{
			using (var db = new DatabaseContext())
			{
				var tag = db.Tags.FirstOrDefault(x => x.Id == id);

				if (tag == null) return new { Error = "Тег не найден." };

				tag.Inputs = db.Rel_Tag_Input
					.Where(x => x.TagId == id)
					.ToList();

				return tag;
			}
		}

		public object Update(Tag tag)
		{
			using (var db = new DatabaseContext())
			{
				if (!db.Tags.Any(x => x.Name == tag.Name)) return new { Error = "Тег не найден." };
				if (tag.Name.Contains(' ')) return Error("В имени тега не разрешены пробелы");
				if (tag.SourceItem.Contains(' ')) return Error("В адресе значения не разрешены пробелы");

				db.Tags
					.Where(x => x.Id == tag.Id)
					.Set(x => x.Name, tag.Name)
					.Set(x => x.Description, tag.Description)
					.Set(x => x.SourceId, tag.SourceId)
					.Set(x => x.SourceItem, tag.SourceItem)
					.Set(x => x.Interval, tag.Interval)
					.Set(x => x.Type, tag.Type)
					.Set(x => x.IsScaling, tag.IsScaling)
					.Set(x => x.MinRaw, tag.MinRaw)
					.Set(x => x.MaxRaw, tag.MaxRaw)
					.Set(x => x.MinEU, tag.MinEU)
					.Set(x => x.MaxEU, tag.MaxEU)
					.Set(x => x.IsCalculating, tag.IsCalculating)
					.Set(x => x.Formula, tag.Formula)
					.Update();

				db.Rel_Tag_Input
					.Where(x => x.TagId == tag.Id)
					.Delete();

				foreach (var input in tag.Inputs)
				{
					db.Rel_Tag_Input
						.Value(x => x.TagId, tag.Id)
						.Value(x => x.InputTagId, input.InputTagId)
						.Value(x => x.VariableName, input.VariableName)
						.Insert();
				}

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
