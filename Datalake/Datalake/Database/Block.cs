using LinqToDB.Mapping;
using System.Collections.Generic;
using System.Linq;

namespace Datalake.Database
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


		// поля для маппинга

		public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

		public List<Rel_Block_Tag> Tags { get; set; } = new List<Rel_Block_Tag>();

		public List<Block> Children { get; set; } = new List<Block>();

		public void LoadChildren(List<Block> all)
		{
			Children.Clear();

			Children = all
				.Where(x => x.ParentId == Id)
				.ToList();

			foreach (var child in Children)
			{
				child.LoadChildren(all);
			}
		}
	}
}
