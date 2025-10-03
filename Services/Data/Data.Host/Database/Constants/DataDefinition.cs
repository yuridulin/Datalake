namespace Datalake.DataService.Database.Constants;

public static class DataDefinition
{
	public const string Schema = "history";

	public static class TagHistory
	{
		public const string Table = "TagHistory";

		public const string TagId = "TagId";
		public const string Date = "Date";
		public const string Text = "Text";
		public const string Number = "Number";
		public const string Quality = "Quality";

		public const string UniqueIndexName = $"{Table}_{TagId}_{Date}_idx";
	}
}
