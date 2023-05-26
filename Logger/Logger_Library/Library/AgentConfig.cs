using System;
using System.Collections.Generic;

namespace Logger.Library
{
	public class AgentConfig
	{
		public DateTime LastUpdate { get; set; }

		public List<AgentLogFilter> Filters { get; set; } = new List<AgentLogFilter>();

		public List<AgentActionPing> Pings { get; set; } = new List<AgentActionPing>();

		public List<AgentActionSql> SqlActions { get; set; } = new List<AgentActionSql>();

		public List<AgentActionNtp> NtpActions { get; set; } = new List<AgentActionNtp>();
	}
}