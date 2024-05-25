using DatalakeDatabase.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeDatabase.ApiModels.Tags;

public class TagUpdateRequest
{
	[Required]
	public required string Name { get; set; }

	public string? Description { get; set; }

	[Required]
	public TagType Type { get; set; }

	[Required]
	public short IntervalInSeconds { get; set; } = 0;

	[Required]
	public int SourceId { get; set; } = (int)CustomSource.Manual;

	[Required]
	public SourceType SourceType { get; set; } = SourceType.Datalake;

	public string? SourceItem { get; set; } = string.Empty;

	public string? Formula { get; set; } = string.Empty;

	[Required]
	public bool IsScaling { get; set; }

	[Required]
	public float MinEu { get; set; } = float.MinValue;

	[Required]
	public float MaxEu { get; set; } = float.MaxValue;

	[Required]
	public float MinRaw { get; set; } = float.MinValue;

	[Required]
	public float MaxRaw { get; set; } = float.MaxValue;

	[Required]
	public TagInputInfo[] FormulaInputs { get; set; } = [];
}
