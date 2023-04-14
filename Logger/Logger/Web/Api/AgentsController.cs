using LinqToDB.Data;
using Logger.Database;
using Logger.Web.Models;
using Logger_Library;
using System.Collections.Generic;
using System.Linq;

namespace Logger.Web.Api
{
	public class AgentsController : Controller
	{
		public object Install(string endpoint)
		{
			return new { Error = "Not implemented" };
		}

		public object Uninstall(string endpoint)
		{
			return new { Error = "Not implemented" };
		}

		public object Reply(List<Log> logs)
		{
			using (var db = new DatabaseContext())
			{
				db.Logs.BulkCopy(logs.Select(x => new LogEntry
				{
					Category = x.Category,
					EventId = x.EventId,
					FilterId = 0,
					JournalName = x.JournalName,
					MachineName = x.MachineName,
					Message = x.Message,
					Source = x.Source,
					TimeGenerated = x.TimeGenerated,
					Type = x.Type,
					Username = x.Username,
				}));
			}

			return new { Done = true };
		}
	}
}
