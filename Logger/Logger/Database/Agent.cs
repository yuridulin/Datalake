using LinqToDB.Mapping;
using System;

namespace Logger.Database
{
	[Table(Name = "Agents")]
	public class Agent
	{
		[Column, NotNull]
		public string MachineName { get; set; }

		[Column, NotNull]
		public int PresetId { get; set; }

		[Column]
		public string Description { get; set; }

		[Column, NotNull]
		public DateTime LastReply { get; set; } = DateTime.MinValue;

		public bool IsOnline => (DateTime.Now - LastReply).TotalSeconds <= 10;
	}
}
