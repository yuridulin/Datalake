using LinqToDB.Mapping;
using System.Collections.Generic;

namespace Logger.Database
{
	[Table(Name = "StationsConfigs")]
	public class StationConfig
	{
		[Column, PrimaryKey]
		public int Id { get;set; }

		[Column]
		public string Name { get; set; }

		[Column]
		public string Description { get; set; }

		// поля для маппинга

		public List<LogFilter> Filters { get; set; } = new List<LogFilter>();

		public List<ActionPing> Pings { get; set; } = new List<ActionPing>();

		public List<ActionSql> ActionsSql { get; set; } = new List<ActionSql>();
	}
}
