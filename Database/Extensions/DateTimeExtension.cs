using Datalake.Database.Enums;

namespace Datalake.Database.Extensions;

/// <summary>
/// Расширение для объектов даты/времени
/// </summary>
public static class DateTimeExtension
{
	/// <summary>
	/// Усечение даты по частоте записи (округление)
	/// </summary>
	/// <param name="dateTime">Оригинальная дата записи</param>
	/// <param name="frequency">Заданная частота записи</param>
	/// <returns>Преобразованная дата</returns>
	public static DateTime RoundToFrequency(this DateTime dateTime, TagFrequency frequency)
	{
		return frequency switch
		{
			TagFrequency.ByDay => dateTime.Date,
			TagFrequency.ByHour => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0, 0),
			TagFrequency.ByMinute => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0, 0),
			_ => dateTime,
		};
	}
}
