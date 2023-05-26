using Datalake.Database;
using Datalake.Web.Models;

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
	}
}
