using LinqToDB.Mapping;

namespace Datalake.Database
{
	[Table(Name = "Rel_Block_Child")]
	public class Rel_Block_Child
	{
		[Column, NotNull]
		public int BlockId { get; set; }

		[Column, NotNull]
		public int ChildId { get; set; }
	}
}
