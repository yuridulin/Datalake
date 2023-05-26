using LinqToDB.Mapping;
using System;

namespace Logger.Database
{
	[Table(Name = "LogsReactions")]
	public class LogReaction
	{
		[Column, Identity]
		public int Id { get; set; }

		[Column]
		public int LogId { get; set; }

		[Column]
		public string Username { get; set; }

		[Column]
		public DateTime Date { get; set; } = DateTime.Now;
	}
}
