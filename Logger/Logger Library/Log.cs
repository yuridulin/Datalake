using System;
using System.Diagnostics;

namespace Logger_Library
{
	public class Log
	{
		public string MachineName { get; set; }

		public string JournalName { get; set; }

		public string Category { get; set; }

		public EventLogEntryType Type { get; set; }

		public int EventId { get; set; }

		public string Message { get; set; }

		public string Source { get; set; }

		public DateTime TimeGenerated { get; set; }

		public string Username { get; set; }

		public string ToConsole()
		{
			return $"{MachineName} {JournalName} {Type} : {EventId}";
		}
	}
}
