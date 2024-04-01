using DatalakeDatabase.Enums;

namespace DatalakeApp.Models
{
	public class HistoryResponse
	{
		public int Id { get; set; }

		public string TagName { get; set; } = string.Empty;

		public TagType Type { get; set; }

		public AggregationFunc Func { get; set; }

		public HistoryRecord[] Values { get; set; } = [];
	}
}
