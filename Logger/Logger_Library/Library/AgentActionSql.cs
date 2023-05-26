using System;
using System.Collections.Generic;

namespace Logger.Library
{
	public class AgentActionSql
	{
		public int Interval { get; set; }

		public string ConnectionString { get; set; }

		public int CommandTimeout { get; set; }

		public string CommandCode { get; set; }

		public string DatabaseType { get; set; }

		public List<AgentActionSqlComparer> Comparers { get; set; } = new List<AgentActionSqlComparer>();

		// Реализация периодичности срабатывания

		DateTime LastExecute { get; set; } = DateTime.MinValue;

		public bool IsTimedOut(DateTime date)
		{
			return Interval < (date - LastExecute).TotalSeconds;
		}

		public void Restart(DateTime date)
		{
			LastExecute = date;
		}
	}
}