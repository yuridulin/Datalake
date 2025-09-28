using System.Text.RegularExpressions;

namespace Datalake.Inventory.Extensions;

/// <summary>
/// Проверки значений
/// </summary>
public static partial class StringExtensions
{
	/// <summary>
	/// Удаление пробелов из строки
	/// </summary>
	/// <param name="value">Строка</param>
	/// <param name="replace">На что заменить пробелы</param>
	/// <returns>Очищенная строка</returns>
	public static string RemoveWhitespaces(this string value, string replace = "") => WhitespacesRegex().Replace(value, replace);

	[GeneratedRegex(@"\s+")]
	private static partial Regex WhitespacesRegex();
}