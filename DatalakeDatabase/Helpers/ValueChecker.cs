using System.Text.RegularExpressions;

namespace DatalakeDatabase.Helpers
{
	public static partial class ValueChecker
	{
		[GeneratedRegex(@"\s+")]
		private static partial Regex WhitespaceFoundRegex();

		public static string RemoveWhitespaces(string value, string replace = "") => WhitespaceFoundRegex().Replace(value, replace);
	}
}
