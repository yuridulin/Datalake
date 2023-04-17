using LinqToDB;
using LinqToDB.Data;
using Logger.Database;
using Logger.Web.Models;
using Logger_Library;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Logger.Web.Api
{
	public class AgentsController : Controller
	{
		public object List()
		{
			using (var db = new DatabaseContext())
			{
				return db.Agents
					.ToList();
			}
		}

		public object Create(string machineName, string description, int presetId)
		{
			using (var db = new DatabaseContext())
			{
				db.Agents
					.Value(x => x.MachineName, machineName.ToUpper())
					.Value(x => x.Description, description)
					.Value(x => x.PresetId, presetId)
					.Value(x => x.LastReply, DateTime.MinValue)
					.Insert();

				return new { Done = "Станция успешно добавлена" };
			}
		}

		public object Read(string machineName)
		{
			using (var db = new DatabaseContext())
			{
				var model = db.Agents
					.FirstOrDefault(x => x.MachineName == machineName.ToUpper());

				if (model == null) return new { Error = "Станция не найдена" };

				return model;
			}
		}

		public object Update(string machineName, string description, int presetId)
		{
			using (var db = new DatabaseContext())
			{
				db.Agents
					.Where(x => x.MachineName == machineName.ToUpper())
					.Set(x => x.Description, description)
					.Set(x => x.PresetId, presetId)
					.Update();

				return new { Done = "Станция успешно сохранена" };
			}
		}

		public object Delete(string machineName)
		{
			using (var db = new DatabaseContext())
			{
				db.Agents
					.Where(x => x.MachineName == machineName.ToUpper())
					.Delete();

				return new { Done = "Станция успешно удалена" };
			}
		}

		public object Install(string machineName)
		{
			return new { Error = "Not implemented" };
		}

		public object Uninstall(string machineName)
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
