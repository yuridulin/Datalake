using LinqToDB.Mapping;

namespace Datalake.Database
{
	[Table(Name = "Rel_Block_Tag")]
	public class Rel_Block_Tag
	{
		[Column, NotNull]
		public int BlockId { get; set; }

		[Column, NotNull]
		public int TagId { get; set; } = 0;

		[Column]
		public string Name { get; set; }
	}
}
