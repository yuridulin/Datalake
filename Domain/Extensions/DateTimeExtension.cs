namespace Datalake.Domain.Extensions;

public static class DateTimeExtension
{
	/// <summary>
	/// dd.MM.yyyy HH:mm:ss
	/// </summary>
	public const string Standart = "dd.MM.yyyy HH:mm:ss";

	/// <summary>
	/// yyyy-MM-dd HH:mm:ss.fff
	/// </summary>
	public const string HierarchicalWithMillisecondsMask = "yyyy-MM-dd HH:mm:ss.fff";

	/// <summary>
	/// dd.MM.yyyy
	/// </summary>
	public const string Date = "dd.MM.yyyy";

	/// <summary>
	/// HH:mm:ss
	/// </summary>
	public const string Time = "HH:mm:ss";

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

	/// <summary>
	/// Преобразование диапазона времени в строковое представление
	/// </summary>
	/// <param name="timeSpan">Диапазон времени</param>
	/// <returns>Строковое представление</returns>
	public static string FormatTimeSpan(TimeSpan timeSpan)
	{
		if (timeSpan == TimeSpan.Zero)
			return "меньше 1 мс";

		if (timeSpan.TotalSeconds < 1)
			return $"{timeSpan.Milliseconds} мс";

		if (timeSpan.TotalMinutes < 1)
			return $"{timeSpan.Seconds} с {timeSpan.Milliseconds} мс";

		if (timeSpan.TotalHours < 1)
			return $"{timeSpan.Minutes} мин {timeSpan.Seconds} с";

		if (timeSpan.TotalDays < 1)
			return $"{timeSpan.Hours}:{timeSpan.Minutes}";

		else
			return timeSpan.ToString();
	}


	public static string HierarchicalWithMilliseconds(this DateTime dateTime) => dateTime.ToString(HierarchicalWithMillisecondsMask);
}
