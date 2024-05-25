using System.Text.RegularExpressions;

namespace DatalakeDatabase.Extensions
{
	public static partial class ValueChecker
	{
		[GeneratedRegex(@"\s+")]
		private static partial Regex WhitespaceFoundRegex();

		public static string RemoveWhitespaces(this string value, string replace = "") => WhitespaceFoundRegex().Replace(value, replace);
	}
}
