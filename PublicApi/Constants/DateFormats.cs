namespace Datalake.PublicApi.Constants;

/// <summary>
/// Маски для перевода дат в строковое представление
/// </summary>
public static class DateFormats
{
	/// <summary>
	/// dd.MM.yyyy HH:mm:ss
	/// </summary>
	public const string Standart = "dd.MM.yyyy HH:mm:ss";

	/// <summary>
	/// yyyy-MM-dd HH:mm:ss.fff
	/// </summary>
	public const string HierarchicalWithMilliseconds = "yyyy-MM-dd HH:mm:ss.fff";

	/// <summary>
	/// Получение текущей даты в выбранном часовом поясе
	/// </summary>
	public static DateTime GetCurrentDateTime()
	{
		// Только попробуйте еще где-то запустить
		TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
		DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
		return localTime;
	}
}
