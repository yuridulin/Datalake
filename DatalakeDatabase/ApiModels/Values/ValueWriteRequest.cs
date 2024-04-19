using DatalakeDatabase.Enums;

namespace DatalakeDatabase.ApiModels.Values;

public class ValueWriteRequest
{
	public int? TagId { get; set; }

	public string? TagName { get; set; }

	public object? Value { get; set; }

	public DateTime Date { get; set; } = DateTime.Now;

	public TagQuality? TagQuality { get; set; }
}
