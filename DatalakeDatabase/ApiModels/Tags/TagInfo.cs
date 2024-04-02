using DatalakeDatabase.Enums;

namespace DatalakeDatabase.ApiModels.Tags
{
	public class TagInfo
	{
		public int? Id { get; set; }

		public required string Name { get; set; }

		public string? Description { get; set; }

		public TagType Type { get; set; }

		public TagSourceInfo? SourceInfo { get; set; }

		public TagMathInfo? MathInfo { get; set; }

		public class TagSourceInfo
		{
			public int Id { get; set; }

			public SourceType Type { get; set; } = SourceType.Datalake;

			public required string Item { get; set; }

			public required string Name { get; set; }
		}

		public class TagMathInfo
		{
			public float MinEu { get; set; }

			public float MaxEu { get; set; }

			public float MinRaw { get; set; }

			public float MaxRaw { get; set; }
		}
	}
}
