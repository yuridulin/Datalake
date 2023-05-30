using LinqToDB.Mapping;
using System.Collections.Generic;

namespace Datalake.Database
{
	[Table(Name = "Blocks")]
	public class Block
	{
		[Column, PrimaryKey, Identity]
		public int Id { get; set; } = 0;

		[Column, NotNull]
		public string Name { get; set; } = string.Empty;

		[Column]
		public string Description { get; set; } = string.Empty;


		// поля для маппинга

		public List<Tag> Properties { get; set; } = new List<Tag>();

		public List<Block> Children { get; set; } = new List<Block>();
	}
}
