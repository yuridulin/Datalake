using System.Collections.Generic;

namespace Datalake.Web.Models
{
	public class TagValuesRange
	{
		public string TagName { get; set; }

		public List<TagValue> Values { get; set; } = new List<TagValue>();
	}
}
