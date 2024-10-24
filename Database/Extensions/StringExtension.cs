using System.Text.RegularExpressions;

namespace Datalake.Database.Extensions;

/// <summary>
/// Проверки значений
/// </summary>
public static partial class ValueChecker
{
	[GeneratedRegex(@"\s+")]
	private static partial Regex WhitespaceFoundRegex();

	/// <summary>
	/// Удаление пробелов из строки
	/// </summary>
	/// <param name="value">Строка</param>
	/// <param name="replace">На что заменить пробелы</param>
	/// <returns>Очищенная строка</returns>
	public static string RemoveWhitespaces(this string value, string replace = "") => WhitespaceFoundRegex().Replace(value, replace);
}
