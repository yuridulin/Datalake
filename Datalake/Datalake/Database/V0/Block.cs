using LinqToDB.Mapping;

namespace Datalake.Database.V0
{
	[Table(Name = "Blocks")]
	public class Block
	{
		[Column, PrimaryKey, Identity]
		public int Id { get; set; } = 0;

		[Column, NotNull]
		public int ParentId { get; set; } = 0;

		[Column, NotNull]
		public string Name { get; set; } = string.Empty;

		[Column]
		public string Description { get; set; } = string.Empty;

		[Column]
		public string PropertiesRaw { get; set; } = string.Empty;
	}
}
