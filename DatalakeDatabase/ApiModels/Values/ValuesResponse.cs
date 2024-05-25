using DatalakeDatabase.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeDatabase.ApiModels.Values;

public class ValuesResponse
{
	[Required]
	public required int Id { get; set; }

	[Required]
	public required string TagName { get; set; }

	[Required]
	public required TagType Type { get; set; }

	[Required]
	public required AggregationFunc Func { get; set; }

	[Required]
	public required ValueRecord[] Values { get; set; } = [];
}
