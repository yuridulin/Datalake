using System;
using System.Collections.Generic;

namespace Logger.Library
{
	public class AgentReply
	{
		public string Endpoint { get; set; }

		public string Version { get; set; }

		public DateTime LastUpdate { get; set; }

		public List<AgentLog> Logs { get; set; }

		public List<AgentSpec> Specs { get; set; }
	}
}