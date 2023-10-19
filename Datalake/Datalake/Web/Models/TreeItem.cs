using System.Collections.Generic;

namespace Datalake.Web.Models
{
	public class TreeItem
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public TreeType Type { get; set; }

		public List<TreeItem> Items { get; set; } = new List<TreeItem>();
	}

	public enum TreeType
	{
		Source,
		TagGroup,
		Tag,
		Block,
		Link,
	}
}
