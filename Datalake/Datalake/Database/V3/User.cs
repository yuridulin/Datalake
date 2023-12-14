using LinqToDB.Mapping;

namespace Datalake.Database.V3
{
	public class User : V1.User
	{
		[Column]
		public string StaticHost { get; set; } = null;
	}
}
