using LinqToDB.Mapping;

namespace Logger.Database
{
	[Table(Name = "Rel_LogFilter_Channel")]
	public class Rel_LogFilter_Channel
	{
		[Column]
		public int LogFilterId { get; set; }

		[Column]
		public int ChannelId { get; set; }
	}
}
