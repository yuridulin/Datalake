using LinqToDB.Mapping;

namespace Datalake.Database.V1
{
	public class User : V0.User
	{
		[Column, NotNull]
		public string FullName { get; set; } = string.Empty;
	}
}
