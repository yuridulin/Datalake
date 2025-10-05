namespace Datalake.Shared.Infrastructure.Schema;

public static class DataSchema
{
	public static string Name { get; } = "data";

	public static class TagsHistory
	{
		public static string Name { get; } = nameof(TagsHistory);

		public static class Columns
		{
			public static string TagId { get; } = nameof(TagId);
			public static string Date { get; } = nameof(Date);
			public static string Text { get; } = nameof(Text);
			public static string Number { get; } = nameof(Number);
			public static string Boolean { get; } = nameof(Boolean);
			public static string Quality { get; } = nameof(Quality);
		}

		public static class Indexes
		{
			public static string UniqueTagIdDateDesc { get; } = $"{Name}_{Columns.TagId}_{Columns.Date}_idx";
		}
	}
}
