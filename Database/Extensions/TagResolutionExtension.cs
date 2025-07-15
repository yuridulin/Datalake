using Datalake.PublicApi.Enums;

namespace Datalake.Database.Extensions;

/// <summary>
/// Расширение для работы с частотой значений
/// </summary>
public static class TagResolutionExtension
{
	static Dictionary<TagResolution, int> Order = new() {
		{ TagResolution.NotSet, 0 },
		{ TagResolution.Second, 1 },
		{ TagResolution.Minute, 2 },
		{ TagResolution.HalfHour, 3 },
		{ TagResolution.Hour, 4 },
		{ TagResolution.Day, 5 },
		{ TagResolution.Week, 6 },
		{ TagResolution.Month, 7 },
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
}
