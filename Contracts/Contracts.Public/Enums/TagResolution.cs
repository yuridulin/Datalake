namespace Datalake.Contracts.Public.Enums;

/// <summary>
/// Частота записи/чтения значения
/// </summary>
public enum TagResolution
{
	/// <summary>
	/// По изменению
	/// </summary>
	None = 0,

	/// <summary>
	/// Минута
	/// </summary>
	Minute = 1,

	/// <summary>
	/// Час
	/// </summary>
	Hour = 2,

	/// <summary>
	/// Сутки
	/// </summary>
	Day = 3,

	/// <summary>
	/// Получас
	/// </summary>
	HalfHour = 4,

	/// <summary>
	/// Неделя
	/// </summary>
	Week = 5,

	/// <summary>
	/// Месяц
	/// </summary>
	Month = 6,

	/// <summary>
	/// Секунда
	/// </summary>
	Second = 7,

	/// <summary>
	/// 3 минуты
	/// </summary>
	Minute3 = 8,

	/// <summary>
	/// 5 минут
	/// </summary>
	Minute5 = 9,

	/// <summary>
	/// 10 минут
	/// </summary>
	Minute10 = 10,

	/// <summary>
	/// 15 минут
	/// </summary>
	Minute15 = 11,

	/// <summary>
	/// 20 минут
	/// </summary>
	Minute20 = 12,

	/// <summary>
	/// Год
	/// </summary>
	Year = 13,

	/// <summary>
	/// Квартал
	/// </summary>
	Quarter = 14,
}
