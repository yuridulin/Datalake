namespace Datalake.Server.Services.Receiver.Models.OldDatalake;

/// <summary>
/// Пришедшее значение
/// </summary>
public class HistoryValue
{
	/// <summary>
	/// Дата записи
	/// </summary>
	public DateTime Date { get; set; } = DateTime.Now;

	/// <summary>
	/// Значение
	/// </summary>
	public object? Value { get; set; }

	/// <summary>
	/// Достоверность
	/// </summary>
	public TagQuality Quality { get; set; }

	/// <summary>
	/// Флаг использования
	/// </summary>
	public TagHistoryUse Using { get; set; }
}
