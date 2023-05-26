using LinqToDB.Mapping;
using System;

namespace Logger.Database
{
	[Table(Name = "Logs")]
	public class Log
	{
		[Column, PrimaryKey]
		public int Id { get; set; } = 0;

		[Column]
		public string Endpoint { get; set; } = string.Empty;

		[Column]
		public DateTime TimeStamp { get; set; } = DateTime.MinValue;

		[Column]
		public int LogFilterId { get; set; } = 0;

		[Column]
		public string Journal { get; set; } = string.Empty;

		[Column]
		public string Source { get; set; } = string.Empty;

		[Column]
		public int EventId { get; set; } = 0;

		[Column]
		public string Category { get; set; } = string.Empty;

		[Column]
		public string Type { get; set; } = string.Empty;

		[Column]
		public string Username { get; set; } = string.Empty;

		[Column]
		public string Message { get; set; } = string.Empty;

		[Column, NotNull]
		public bool Checked { get; set; } = false;
	}
}
