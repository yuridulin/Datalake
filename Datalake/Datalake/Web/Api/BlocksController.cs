using Datalake.Database;
using Datalake.Web.Models;
using LinqToDB;
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

				block.Children = db.Blocks
					.Where(x => x.ParentId == block.Id)
					.ToList();
				
				return block;
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

				db.SetUpdateDate();

				return Done("Объект создан");
			}
		}

		public object Update(Block block, List<Rel_Block_Tag> tags)
		{
			using (var db = new DatabaseContext())
			{
				if (db.Blocks.Any(x => x.Id == block.Id)) return Error("Объект с Id = " + block.Id + " не найден");

				db.Blocks
					.Where(x => x.Id == block.Id)
					.Set(x => x.Name, block.Name)
					.Set(x => x.Description, block.Description)
					.Set(x => x.PropertiesRaw, block.PropertiesRaw)
					.Update();

				db.Rel_Block_Tag
					.Where(x => x.BlockId == block.Id)
					.Delete();

				foreach (var tag in tags)
				{
					db.Rel_Block_Tag
						.Value(x => x.BlockId, block.Id)
						.Value(x => x.Name, tag.Name)
						.Value(x => x.TagId, tag.TagId)
						.Value(x => x.Type, tag.Type)
						.Insert();
				}

				db.SetUpdateDate();

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

				db.SetUpdateDate();

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

				db.SetUpdateDate();

				return Done("Объект удалён");
			}
		}
	}
}
