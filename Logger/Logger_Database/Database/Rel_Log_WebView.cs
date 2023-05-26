using LinqToDB.Mapping;

namespace Logger.Database
{
	[Table(Name = "Rel_Log_WebView")]
	public class Rel_Log_WebView
	{
		[Column]
		public int LogId { get; set; }

		[Column]
		public int ChannelId { get; set; }
	}
}
