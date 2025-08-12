namespace Datalake.Server.Services.Receiver.Models.OldDatalake;

/// <summary>
/// Достоверность
/// </summary>
public enum TagQuality
{
	/// <summary>
	/// Недостоверно
	/// </summary>
	Bad = 0,
	/// <summary>
	/// Нет подключения
	/// </summary>
	Bad_NoConnect = 4,
	/// <summary>
	/// Нет значений
	/// </summary>
	Bad_NoValues = 8,
	/// <summary>
	/// Нет значений, записано вручную
	/// </summary>
	Bad_ManualWrite = 26,
	/// <summary>
	/// Достоверно
	/// </summary>
	Good = 192,
	/// <summary>
	/// Достоверно, записано вручную
	/// </summary>
	Good_ManualWrite = 216,
	/// <summary>
	/// Неизвестно
	/// </summary>
	Unknown = -1,
}
