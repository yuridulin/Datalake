using System;

namespace Logger.Library
{
	public class AgentLog
	{
		public string Endpoint { get; set; }

		public DateTime TimeStamp { get; set; }

		public int LogFilterId { get; set; }

		public string Journal { get; set; }

		public string Source { get; set; }

		public int EventId { get; set; }

		public string Category { get; set; }

		public string Type { get; set; }

		public string Username { get; set; }

		public string Message { get; set; }
	}
}
