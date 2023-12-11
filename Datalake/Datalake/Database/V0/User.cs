using Datalake.Enums;
using LinqToDB.Mapping;

namespace Datalake.Database.V0
{
	[Table(Name = "Users")]
	public class User
	{
		[Column, NotNull]
		public string Name { get; set; } = string.Empty;

		[Column, NotNull]
		public string Hash { get; set; } = string.Empty;

		[Column, NotNull]
		public AccessType AccessType { get; set; } = AccessType.USER;
	}
}
