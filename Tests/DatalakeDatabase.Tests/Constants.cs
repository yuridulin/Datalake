using DatalakeApiClasses.Models.Users;

namespace DatalakeDatabase.Tests
{
	public static class Constants
	{
		public static Guid TagGuid { get; set; }
		public const string TagName = "test";

		public static readonly DateTime FirstWriteDate = DateTime.Parse("2024-01-07 11:11:11");
		public const float FirstWriteValue = 111;

		public static readonly DateTime AfterFirstWriteDate = DateTime.Parse("2024-01-16 13:13:13");
		public const float AfterFirstWriteValue = 333;

		public static readonly DateTime InCenterDate = DateTime.Parse("2024-01-19 11:12:11");
		public const float InCenterValue = AfterFirstWriteValue;

		public static readonly DateTime BeforeLastWriteDate = DateTime.Parse("2024-02-04 12:12:12");
		public const float BeforeLastWriteValue = 222;

		public const float LastValue = 666;

		public static UserAuthInfo? DefaultAdmin { get; set; }
	}
}
