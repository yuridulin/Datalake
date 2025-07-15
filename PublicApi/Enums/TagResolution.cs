namespace Datalake.PublicApi.Enums;

/// <summary>
/// Частота записи/чтения значения
/// </summary>
public enum TagResolution
{
	/// <summary>
	/// Неопределенная частота (произвольные моменты/по изменению)
	/// </summary>
	NotSet = 0,

	/// <summary>
	/// Поминутная частота
	/// </summary>
	ByMinute = 1,

	/// <summary>
	/// Почасовая частота
	/// </summary>
	ByHour = 2,

	/// <summary>
	/// Посуточная частота
	/// </summary>
	ByDay = 3,

	/// <summary>
	/// Получасы (30 минут)
	/// </summary>
	ByHalfHour = 4,

	/// <summary>
	/// Понедельная частота
	/// </summary>
	ByWeek = 5,

	/// <summary>
	/// Помесячная частота
	/// </summary>
	ByMonth = 6,

	/// <summary>
	/// Посекундная частота
	/// </summary>
	BySecond = 7,
}
