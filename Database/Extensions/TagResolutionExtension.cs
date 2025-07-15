using Datalake.PublicApi.Enums;

namespace Datalake.Database.Extensions;

/// <summary>
/// Расширение для работы с частотой значений
/// </summary>
public static class TagResolutionExtension
{
	static Dictionary<TagResolution, int> Order = new() {
		{ TagResolution.NotSet, 0 },
		{ TagResolution.BySecond, 1 },
		{ TagResolution.ByMinute, 2 },
		{ TagResolution.ByHalfHour, 3 },
		{ TagResolution.ByHour, 4 },
		{ TagResolution.ByDay, 5 },
		{ TagResolution.ByWeek, 6 },
		{ TagResolution.ByMonth, 7 },
	};

	/// <summary>
	/// Получение порядка сортировки, от меньшего к большему
	/// </summary>
	/// <param name="resolution">Текущая частота</param>
	/// <returns>Порядковый номер</returns>
	public static int GetSortOrder(this TagResolution resolution)
	{
		return Order[resolution];
	}

	/// <summary>
	/// Применение частоты к дате, чтобы получить следующую дату
	/// </summary>
	/// <param name="resolution"></param>
	/// <param name="date"></param>
	/// <returns></returns>
	public static DateTime AddToDate(this TagResolution resolution, DateTime date)
	{
		return resolution switch
		{
			TagResolution.BySecond => date.AddSeconds(1),
			TagResolution.ByMinute => date.AddMinutes(1),
			TagResolution.ByHalfHour => date.AddMinutes(30),
			TagResolution.ByHour => date.AddHours(1),
			TagResolution.ByDay => date.AddDays(1),
			TagResolution.ByWeek => date.AddDays(7),
			TagResolution.ByMonth => date.AddMonths(1),
			_ => date,
		};
	}
}
