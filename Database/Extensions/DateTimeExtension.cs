using Datalake.PublicApi.Enums;

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
	/// <param name="resolution">Заданная частота записи</param>
	/// <returns>Преобразованная дата</returns>
	public static DateTime RoundByResolution(this DateTime dateTime, TagResolution resolution)
	{
		return resolution switch
		{
			TagResolution.ByMonth => new DateTime(dateTime.Year, dateTime.Month, 1),
			TagResolution.ByWeek => dateTime.Date.AddDays(-(dateTime.DayOfWeek == DayOfWeek.Sunday ? 7 : dateTime.DayOfWeek - DayOfWeek.Monday)),
			TagResolution.ByDay => dateTime.Date,
			TagResolution.ByHalfHour => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute >= 30 ? 30 : 0, 0, 0),
			TagResolution.ByHour => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0, 0),
			TagResolution.ByMinute => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0, 0),
			TagResolution.BySecond => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, 0),
			_ => dateTime,
		};
	}
}
