using LinqToDB.Mapping;
using System.Collections.Generic;

namespace Logger.Database
{
	[Table(Name = "Channels")]
	public class Channel
	{
		[Column, Identity]
		public int Id { get; set; }

		[Column]
		public string Name { get; set; }

		[Column]
		public string Type { get; set; }

		// маппинг

		public List<LogFilter> Filters { get; set; } = null;
	}
}
