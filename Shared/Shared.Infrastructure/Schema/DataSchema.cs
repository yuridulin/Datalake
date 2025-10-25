namespace Datalake.Shared.Infrastructure.Schema;

public static class DataSchema
{
	public static string Name { get; } = "data";
	public static string Migrations { get; } = "__EFMigrationsHistory";

	public static class TagsValues
	{
		public static string Name { get; } = "TagsValues";

		public static class Columns
		{
			public static string TagId { get; } = "TagId";
			public static string Date { get; } = "Date";
			public static string Text { get; } = "Text";
			public static string Number { get; } = "Number";
			public static string Boolean { get; } = "Boolean";
			public static string Quality { get; } = "Quality";
		}
	}
}
