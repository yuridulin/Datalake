using Datalake.Enums;
using System.Collections.Generic;

namespace Datalake.Models
{
	public class TreeItem
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string FullName { get; set; }

		public TreeType Type { get; set; }

		public List<TreeItem> Items { get; set; } = new List<TreeItem>();
	}
}
