using LinqToDB.Mapping;

namespace Datalake.Database
{
	[Table(Name = "Settings")]
	public class Settings
	{
		[Column, NotNull]
		public string Key { get; set; } = string.Empty;

		[Column]
		public string Value { get; set; }
	}
}
