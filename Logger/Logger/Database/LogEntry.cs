using LinqToDB.Mapping;
using System;
using System.Diagnostics;

namespace Logger.Database
{
	[Table(Name = "Logs")]
	public class LogEntry
	{
		[Column, Identity]
		public long Id { get; set; }

		[Column, NotNull]
		public string MachineName { get; set; }

		[Column, NotNull]
		public string JournalName { get; set; }

		[Column]
		public string Category { get; set; }

		[Column, NotNull]
		public EventLogEntryType Type { get; set; }

		[Column, NotNull]
		public int EventId { get; set; }

		[Column]
		public string Message { get; set; }

		[Column]
		public string Source { get; set; }

		[Column, NotNull]
		public DateTime TimeGenerated { get; set; }

		[Column]
		public string Username { get; set; }

		[Column, NotNull]
		public int FilterId { get; set; }
	}
}
