using DatalakeDatabase.Enums;

namespace DatalakeDatabase.ApiModels.Values;

public class ValueRecord
{
	public DateTime Date { get; set; } = DateTime.Now;

	public object? Value { get; set; }

	public TagQuality Quality { get; set; }

	public TagUsing Using { get; set; }
}
