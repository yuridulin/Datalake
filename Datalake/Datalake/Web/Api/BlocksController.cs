using Datalake.Database;
using Datalake.Web.Models;
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
	}
}
