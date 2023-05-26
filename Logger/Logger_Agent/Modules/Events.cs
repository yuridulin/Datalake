using Logger.Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Logger.Agent.Modules
{
	public static class Events
	{
		static List<EventLog> Journals { get; set; } = new List<EventLog>();

		public static void Start()
		{
			foreach (var eventLog in EventLog.GetEventLogs())
			{
				eventLog.EntryWritten += (s, e) =>
				{
					FilterAndAddLog(new AgentLog
					{
						Endpoint = Environment.MachineName.ToUpper(),
						Journal = eventLog.LogDisplayName,
						LogFilterId = 0,
						Type = e.Entry.EntryType.ToString(),
						Category = e.Entry.Category,
						EventId = e.Entry.EventID,
						Source = e.Entry.Source,
						TimeStamp = e.Entry.TimeWritten,
						Username = e.Entry.UserName,
						Message = e.Entry.Message,
					});
				};

				eventLog.EnableRaisingEvents = true;

				Journals.Add(eventLog);
			}
		}

		public static void Stop()
		{
			foreach (var eventLog in Journals)
			{
				eventLog.EnableRaisingEvents = false;
			}
		}

		public static void FilterAndAddLog(AgentLog log)
		{
			if (Program.Config.Filters.Count == 0) return;

			foreach (var filter in Program.Config.Filters)
			{
				bool passed = filter.Pass(log);

				if (passed) 
				{
					if (!filter.Allow) 
					{
						return;
					}
					else
					{
						log.LogFilterId = filter.Id;
					}
				}
			}

			if (log.LogFilterId != 0)
			{
				Sender.AddLog(log);
			}
		}
	}
}