using DatalakeDb.Enums;

namespace DatalakeDb.Classes
{
	public class HistoryResponse
	{
		public uint Id { get; set; }

		public string TagName { get; set; } = string.Empty;

		public TagType Type { get; set; }

		public AggregationFunc Func { get; set; }

		public List<HistoryRecord> Values { get; set; } = [];
	}
}
