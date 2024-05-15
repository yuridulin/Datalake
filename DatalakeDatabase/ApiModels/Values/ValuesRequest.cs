using DatalakeDatabase.Enums;

namespace DatalakeDatabase.ApiModels.Values;

public class ValuesRequest
{
	public int[]? Tags { get; set; } = [];

	public string[]? TagNames { get; set; } = [];

	public DateTime? Old { get; set; }

	public DateTime? Young { get; set; }

	public DateTime? Exact { get; set; }

	public int? Resolution { get; set; } = 0;

	public AggregationFunc? Func { get; set; } = AggregationFunc.List;
}

