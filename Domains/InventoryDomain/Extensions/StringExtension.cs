using System.Text.RegularExpressions;

namespace Datalake.Database.Extensions;

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

	internal static string MapDate(this string source, string token, DateTime date) => source.Replace(token, date.ToString("yyyy-MM-dd HH:mm:ss.fff"));

	internal static string MapIdentifiers(this string source, int[] identifiers) => source.Replace("@tags", string.Join(',', identifiers));

	/// <summary>
	/// Обрезка версии до первых двух цифр (major.minor)
	/// </summary>
	/// <param name="version">Текущая версия</param>
	/// <returns>Усеченная версия</returns>
	public static string ShortVersion(this string version)
	{
		if (string.IsNullOrWhiteSpace(version))
			return version ?? string.Empty;

		var parts = version.Split('.');
		return parts.Length >= 2
			? $"{parts[0]}.{parts[1]}"
			: version;
	}
}