using Datalake.Database.Enums;
using System.Collections.Generic;

namespace Datalake.Collector.Models
{
	public class TagValuesRange
	{
		public string TagName { get; set; }

		public TagType TagType { get; set; }

		public List<TagValue> Values { get; set; } = new List<TagValue>();
	}
}
