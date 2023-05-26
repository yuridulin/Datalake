using LinqToDB.Mapping;
using System.Collections.Generic;

namespace Logger.Database
{
	[Table(Name = "LogsFilters")]
	public class LogFilter
	{
		[Column, PrimaryKey]
		public int Id { get;set; }

		[Column]
		public string Name { get; set; }

		[Column]
		public string Description { get; set; }

		[Column]
		public bool Allow { get; set; }

		[Column]
		public string Endpoints { get; set; }

		[Column]
		public string Journals { get; set; }

		[Column]
		public string Sources { get; set; }

		[Column]
		public string EventIds { get; set; }

		[Column]
		public string Categories { get; set; }

		[Column]
		public string Types { get; set; }

		
		// Маппинг

		public List<Channel> Channels { get; set; } = new List<Channel>();
	}
}
