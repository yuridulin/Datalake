using Datalake.PublicApi.Enums;

namespace Datalake.DataService.Extensions;

/// <summary>
/// Расширение для объектов даты/времени
/// </summary>
public static class DateTimeExtension
{
	static DateTime Floor(this DateTime dt, TimeSpan span) =>
		new(dt.Ticks - (dt.Ticks % span.Ticks), dt.Kind);

	/// <summary>
	/// Усечение даты по частоте записи (округление)
	/// </summary>
	/// <param name="dt">Оригинальная дата</param>
	/// <param name="res">Заданная частота</param>
	/// <returns>Преобразованная дата</returns>
	public static DateTime RoundByResolution(this DateTime dt, TagResolution res) =>
		res switch
		{
			TagResolution.Second => dt.Floor(TimeSpan.FromSeconds(1)),
			TagResolution.Minute => dt.Floor(TimeSpan.FromMinutes(1)),
			TagResolution.Minute3 => dt.Floor(TimeSpan.FromMinutes(3)),
			TagResolution.Minute5 => dt.Floor(TimeSpan.FromMinutes(5)),
			TagResolution.Minute10 => dt.Floor(TimeSpan.FromMinutes(10)),
			TagResolution.Minute15 => dt.Floor(TimeSpan.FromMinutes(15)),
			TagResolution.Minute20 => dt.Floor(TimeSpan.FromMinutes(20)),
			TagResolution.HalfHour => dt.Floor(TimeSpan.FromMinutes(30)),
			TagResolution.Hour => dt.Floor(TimeSpan.FromHours(1)),
			TagResolution.Day => dt.Floor(TimeSpan.FromDays(1)),
			TagResolution.Week => dt.Date.AddDays(-((int)dt.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7),
			TagResolution.Month => new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, dt.Kind),
			_ => dt
		};

	/// <summary>
	/// Применение частоты к дате, чтобы получить следующую дату
	/// </summary>
	/// <param name="dt">Оригинальная дата</param>
	/// <param name="res">Заданная частота</param>
	/// <returns></returns>
	public static DateTime AddByResolution(this DateTime dt, TagResolution res)
	{
		return res switch
		{
			TagResolution.Second => dt.AddSeconds(1),
			TagResolution.Minute => dt.AddMinutes(1),
			TagResolution.Minute3 => dt.AddMinutes(3),
			TagResolution.Minute5 => dt.AddMinutes(5),
			TagResolution.Minute10 => dt.AddMinutes(10),
			TagResolution.Minute15 => dt.AddMinutes(15),
			TagResolution.Minute20 => dt.AddMinutes(20),
			TagResolution.HalfHour => dt.AddMinutes(30),
			TagResolution.Hour => dt.AddHours(1),
			TagResolution.Day => dt.AddDays(1),
			TagResolution.Week => dt.AddDays(7),
			TagResolution.Month => dt.AddMonths(1),
			_ => dt,
		};
	}
}
