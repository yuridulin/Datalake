using DatalakeDb.Enums;

namespace DatalakeDb.Classes
{
	public class HistoryRecord
	{
		public DateTime Date { get; set; } = DateTime.Now;

		public object? Value { get; set; }

		public TagQuality Quality { get; set; }

		public TagUsing Using { get; set; }
	}
}
