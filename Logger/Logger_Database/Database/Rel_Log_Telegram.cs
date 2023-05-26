using LinqToDB.Mapping;

namespace Logger.Database
{
	[Table(Name = "Rel_Log_Telegram")]
	public class Rel_Log_Telegram
	{
		[Column]
		public int LogId { get; set; }

		[Column]
		public int ChannelId { get; set; }

		[Column]
		public bool IsSended { get; set; } = false;
	}
}
