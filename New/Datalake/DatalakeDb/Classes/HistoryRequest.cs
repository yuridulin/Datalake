using DatalakeDb.Enums;

namespace DatalakeDb.Classes
{
	public class HistoryRequest : LiveRequest
	{
		public DateTime? Old { get; set; }

		public DateTime? Young { get; set; }

		public DateTime? Exact { get; set; }

		public int Resolution { get; set; } = 0;

		public AggregationFunc Func { get; set; } = AggregationFunc.List;
	}
}
