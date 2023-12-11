using System.Collections.Generic;
using System.Linq;

namespace Datalake.Database
{
	public class Block : V0.Block
	{
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
