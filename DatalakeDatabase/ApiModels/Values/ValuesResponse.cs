using DatalakeDatabase.Enums;

namespace DatalakeDatabase.ApiModels.Values;

public class ValuesResponse
{
	public int Id { get; set; }

	public string TagName { get; set; } = string.Empty;

	public TagType Type { get; set; }

	public AggregationFunc Func { get; set; }

	public ValueRecord[] Values { get; set; } = [];
}
