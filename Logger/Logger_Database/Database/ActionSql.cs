using LinqToDB.Mapping;
using Logger.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Logger.Database
{
	[Table(Name = "ActionsSql")]
	public class ActionSql
	{
		[Column, Identity]
		public int Id { get; set; }

		[Column]
		public string Name { get; set; }

		[Column]
		public string Description { get; set; }

		[Column]
		public int Interval { get; set; }

		[Column]
		public string DatabaseType { get; set; }

		[Column]
		public string ConnectionString { get; set; }

		[Column]
		public int CommandTimeout { get; set; }

		[Column]
		public string CommandCode { get; set; }

		[Column]
		public string ComparersJson { get; set; }

		public List<AgentActionSqlComparer> Comparers()
		{
			try
			{
				return JsonConvert.DeserializeObject<List<AgentActionSqlComparer>>(ComparersJson);
			}
			catch (Exception ex)
			{ 
				Helpers.RaiseServerEvent(ServerLogSources.Cache, ex.Message, true);

				return new List<AgentActionSqlComparer>();
			}
		}
	}
}
