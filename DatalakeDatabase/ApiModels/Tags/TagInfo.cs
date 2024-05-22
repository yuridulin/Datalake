using DatalakeDatabase.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeDatabase.ApiModels.Tags;

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
	public required TagSourceInfo SourceInfo { get; set; }

	[Required]
	public required TagMathInfo? MathInfo { get; set; }

	[Required]
	public required TagCalcInfo? CalcInfo { get; set; }


	public class TagSourceInfo
	{
		[Required]
		public int Id { get; set; }

		public SourceType? Type { get; set; } = SourceType.Datalake;

		public string? Item { get; set; }

		public string? Name { get; set; } = string.Empty;
	}

	public class TagMathInfo
	{
		[Required]
		public bool IsScaling { get; set; }

		[Required]
		public float MinEu { get; set; }

		[Required]
		public float MaxEu { get; set; }

		[Required]
		public float MinRaw { get; set; }

		[Required]
		public float MaxRaw { get; set; }
	}

	public class TagCalcInfo
	{
		public string Formula { get; set; } = string.Empty;

		[Required]
		public Dictionary<string, int> Inputs { get; set; } = [];
	}
}
