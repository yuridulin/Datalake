using Datalake.Database;
using Datalake.Web.Models;
using LinqToDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Datalake.Web.Api
{
	public class BlocksController : Controller
	{
		public object List()
		{
			using (var db = new DatabaseContext())
			{
				var blocks = db.Blocks.ToList();

				var top = blocks
					.Where(block => block.ParentId == 0)
					.ToList();

				foreach (var block in top)
				{
					block.LoadChildren(blocks);
				}

				return top;
			}
		}

		public object Read(int Id)
		{
			using (var db = new DatabaseContext())
			{
				var block = db.Blocks.FirstOrDefault(x => x.Id == Id);

				if (block == null) return new { Error = "Не найден объект по Id = " + Id };

				block.Properties = JsonConvert.DeserializeObject<Dictionary<string, string>>(block.PropertiesRaw);

				block.Tags = db.Rel_Block_Tag
					.Where(x => x.BlockId == Id)
					.OrderBy(x => x.Type)
					.ThenBy(x => x.Name)
					.ToList();

				block.Children = db.Blocks
					.Where(x => x.ParentId == block.Id)
					.OrderBy(x => x.Name)
					.ToList();
				
				return block;
			}
		}

		public object Parents(int Id)
		{
			using (var db = new DatabaseContext())
			{
				var list = db.Blocks.Where(x => x.Id != Id).ToList();
				var excluded = new List<int>();

				Process(Id);

				void Process(int blockId)
				{
					var children = list
						.Where(x => x.ParentId == blockId)
						.Select(x => x.Id)
						.ToList();

					excluded.AddRange(children);

					foreach (var child in children)
					{
						Process(child);
					}
				}

				return list.Select(x => x.Id).Except(excluded);
			}
		}

		public List<TagValue> Live(int Id)
		{
			using (var db = new DatabaseContext())
			{
				var tags = db.Rel_Block_Tag
					.Where(x => x.BlockId == Id)
					.ToDictionary(x => x.TagId, x => x.Name);

				var dbtags = db.Tags
					.Where(x => tags.Keys.Contains(x.Id))
					.ToDictionary(x => x.Id, x => x);

				return tags.Keys
					.Select(x => new
					{
						Tag = dbtags[x],
						Value = Cache.Read(x),
					})
					.Select(x => new TagValue
					{
						TagId = x.Tag.Id,
						TagName = tags[x.Tag.Id],
						Type = x.Tag.Type,
						Date = x.Value.Date,
						Value = x.Value.Value(),
						Quality = x.Value.Quality,
						Using = x.Value.Using,
					})
					.ToList();
			}
		}

		public List<TagValue> History(int Id, DateTime old, DateTime young, int resolution)
		{
			using (var db = new DatabaseContext())
			{
				var tags = db.Rel_Block_Tag
					.Where(x => x.BlockId == Id)
					.ToDictionary(x => x.TagId, x => x.Name);

				var dbtags = db.Tags
					.Where(x => tags.Keys.Contains(x.Id))
					.ToDictionary(x => x.Id, x => x);

				var data = db.ReadHistory(tags.Keys.ToArray(), old, young, resolution);

				return data
					.Select(x => new TagValue
					{
						TagId = x.TagId,
						TagName = tags[x.TagId],
						Date = x.Date,
						Using = x.Using,
						Quality = x.Quality,
						Type = x.Type,
						Value = x.Value(),
					})
					.ToList();
			}
		}

		public object Create(int ParentId)
		{
			using (var db = new DatabaseContext())
			{
				db.Blocks
					.Value(x => x.ParentId, ParentId)
					.Value(x => x.Name, "Новый объект")
					.Value(x => x.Description, "")
					.Value(x => x.PropertiesRaw, "{}")
					.Insert();

				Cache.Update();

				return Done("Объект создан");
			}
		}

		public object Update(Block block)
		{
			using (var db = new DatabaseContext())
			{
				if (!db.Blocks.Any(x => x.Id == block.Id)) return Error("Объект с Id = " + block.Id + " не найден");

				db.Blocks
					.Where(x => x.Id == block.Id)
					.Set(x => x.Name, block.Name)
					.Set(x => x.Description, block.Description)
					.Set(x => x.PropertiesRaw, JsonConvert.SerializeObject(block.Properties))
					.Update();

				db.Rel_Block_Tag
					.Where(x => x.BlockId == block.Id)
					.Delete();

				foreach (var tag in block.Tags)
				{
					db.Rel_Block_Tag
						.Value(x => x.BlockId, block.Id)
						.Value(x => x.Name, tag.Name)
						.Value(x => x.TagId, tag.TagId)
						.Value(x => x.Type, tag.Type)
						.Insert();
				}

				Cache.Update();

				return Done("Объект сохранён");
			}
		}

		public object Move(int Id, int ParentId)
		{
			using (var db = new DatabaseContext())
			{
				db.Blocks
					.Where(x => x.Id == Id)
					.Set(x => x.ParentId, ParentId)
					.Update();

				Cache.Update();

				return Done("Объект перемещён");
			}
		}

		public object Delete(int Id)
		{
			using (var db = new DatabaseContext())
			{
				db.Blocks
					.Where(x => x.Id == Id)
					.Delete();

				db.Blocks
					.Where(x => x.ParentId == Id)
					.Set(x => x.ParentId, 0)
					.Update();

				Cache.Update();

				return Done("Объект удалён");
			}
		}
	}
}
