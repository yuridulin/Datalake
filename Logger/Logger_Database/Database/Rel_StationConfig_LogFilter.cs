using LinqToDB.Mapping;

namespace Logger.Database
{
	[Table(Name = "Rel_StationConfig_LogFilter")]
	public class Rel_StationConfig_LogFilter
	{
		[Column]
		public int LogFilterId { get; set; }

		[Column]
		public int StationConfigId { get; set; }
	}
}
