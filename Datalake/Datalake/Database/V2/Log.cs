using LinqToDB.Mapping;

namespace Datalake.Database.V2
{
	public class Log : V0.Log
	{
		[Column]
		public string User { get; set; } = null;
	}
}
