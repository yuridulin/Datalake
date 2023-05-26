using LinqToDB.Mapping;

namespace Logger.Database
{
	[Table(Name = "Rel_StationConfig_ActionSql")]
	public class Rel_StationConfig_ActionSql
	{
		[Column]
		public int StationConfigId { get; set; }

		[Column]
		public int ActionSqlId { get; set; }
	}
}
