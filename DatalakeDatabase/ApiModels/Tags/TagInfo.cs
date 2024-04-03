using DatalakeDatabase.Enums;

namespace DatalakeDatabase.ApiModels.Tags
{
	public class TagInfo
	{
		public int? Id { get; set; }

		public required string Name { get; set; }

		public string? Description { get; set; }

		public TagType Type { get; set; }

		public short? Interval { get; set; }

		public required TagSourceInfo SourceInfo { get; set; }

		public TagMathInfo? MathInfo { get; set; } = null;

		public TagCalcInfo? CalcInfo { get; set; } = null;


		public class TagSourceInfo
		{
			public int Id { get; set; }

			public SourceType Type { get; set; } = SourceType.Datalake;

			public string? Item { get; set; }

			public string Name { get; set; } = string.Empty;
		}

		public class TagMathInfo
		{
			public float MinEu { get; set; }

			public float MaxEu { get; set; }

			public float MinRaw { get; set; }

			public float MaxRaw { get; set; }
		}

		public class TagCalcInfo
		{
			public string Formula { get; set; } = string.Empty;

			public Dictionary<int, string> Inputs { get; set; } = [];
		}
	}
}
