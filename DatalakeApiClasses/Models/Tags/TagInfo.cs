using DatalakeApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Tags;

public class TagInfo
{
	[Required]
	public required int Id { get; set; }

	[Required]
	public required string Name { get; set; }

	public string? Description { get; set; }

	[Required]
	public required TagType Type { get; set; }

	[Required]
	public required short IntervalInSeconds { get; set; }

	[Required]
	public required int SourceId { get; set; }

	[Required]
	public required SourceType SourceType { get; set; } = SourceType.Datalake;

	public string? SourceItem { get; set; }

	public string? SourceName { get; set; } = string.Empty;

	public string? Formula { get; set; } = string.Empty;

	[Required]
	public required bool IsScaling { get; set; }

	[Required]
	public required float MinEu { get; set; }

	[Required]
	public required float MaxEu { get; set; }

	[Required]
	public required float MinRaw { get; set; }

	[Required]
	public required float MaxRaw { get; set; }

	[Required]
	public required Dictionary<string, int> FormulaInputs { get; set; } = [];
}
