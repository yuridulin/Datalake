namespace Datalake.Server.Services.Receiver.Models.OldDatalake;

/// <summary>
/// Флаг использования
/// </summary>
public enum TagHistoryUse
{
	/// <summary>
	/// Начальное значение
	/// </summary>
	Initial = 0,
	/// <summary>
	/// Основное значение
	/// </summary>
	Basic = 1,
	/// <summary>
	/// Результат агрегирования
	/// </summary>
	Aggregated = 2,
	/// <summary>
	/// Протянутое значение
	/// </summary>
	Continuous = 3,
	/// <summary>
	/// Устаревшее значение
	/// </summary>
	Outdated = 100,
	/// <summary>
	/// Значение не существует
	/// </summary>
	NotFound = 101,
}
