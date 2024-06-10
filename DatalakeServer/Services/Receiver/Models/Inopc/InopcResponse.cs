namespace DatalakeServer.Services.Receiver.Models.Inopc;

/// <summary>
/// Ответ INOPC на запрос за значениями
/// </summary>
public class InopcResponse
{
	/// <summary>
	/// Дата получения запроса
	/// </summary>
	public DateTime Timestamp { get; set; }

	/// <summary>
	/// Список запрошенных значений
	/// </summary>
	public required InopcTag[] Tags { get; set; }
}
