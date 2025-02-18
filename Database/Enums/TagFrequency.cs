namespace Datalake.Database.Enums;

/// <summary>
/// Частота записи значения
/// </summary>
public enum TagFrequency
{
	/// <summary>
	/// Неопределенная частота
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
}
