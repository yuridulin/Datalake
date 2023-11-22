using Datalake.Enums;
using System.Collections.Generic;

namespace Datalake.Models
{
	public class HistoryResponse
	{
		public int Id { get; set; }

		public string TagName { get; set; }

		public TagType Type { get; set; }

		public AggFunc Func { get; set; }

		public List<HistoryValue> Values { get; set; } = new List<HistoryValue>();
	}
}
