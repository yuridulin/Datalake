using Datalake.Database;
using Datalake.Web.Models;
using System.Collections.Generic;
using System.Linq;

namespace Datalake.Web.Api
{
	public class ConfigController : Controller
	{
		public object LastUpdate()
		{
			using (var db = new DatabaseContext())
			{
				return db.GetUpdateDate();
			}
		}

		public object Tree()
		{
			using (var db = new DatabaseContext())
			{
				var sources = db.Sources
					.ToList();

				var blocks = db.Blocks
					.ToList();

				return new
				{
					sources,
					blocks = Children(0, blocks),
				};
			}

			List<Block> Children(int id, List<Block> all)
			{
				var children = all
					.Where(x => x.ParentId == id)
					.ToList();

				foreach (var child in children) child.Children = Children(child.Id, all);

				return children;
			}
		}
	}
}
