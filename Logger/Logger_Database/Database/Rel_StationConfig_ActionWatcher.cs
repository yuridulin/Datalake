using LinqToDB.Mapping;

namespace Logger.Database
{
	[Table(Name = "Rel_StationConfig_ActionWatcher")]
	public class Rel_StationConfig_ActionWatcher
	{
		[Column]
		public int StationConfigId { get; set; }

		[Column]
		public int WatcherActionId { get; set; }
	}
}
