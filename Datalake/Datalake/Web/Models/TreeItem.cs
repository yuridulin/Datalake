using System.Collections.Generic;

namespace Datalake.Web.Models
{
	public class TreeItem
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string FullName { get; set; }

		public TreeType Type { get; set; }

		public List<TreeItem> Items { get; set; } = new List<TreeItem>();
	}

	public enum TreeType
	{
		Source = 0,
		TagGroup = 1,
		Tag = 2,
		Block = 3,
		Link = 4,
	}
}
