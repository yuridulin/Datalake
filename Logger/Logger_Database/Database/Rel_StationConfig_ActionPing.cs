using LinqToDB.Mapping;

namespace Logger.Database
{
	[Table(Name = "Rel_StationConfig_ActionPing")]
	public class Rel_StationConfig_ActionPing
	{
		[Column]
		public int StationConfigId { get; set; }

		[Column]
		public int PingActionId { get; set; }
	}
}
