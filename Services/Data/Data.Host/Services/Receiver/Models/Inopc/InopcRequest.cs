namespace Datalake.Data.Host.Services.Receiver.Models.Inopc;

/// <summary>
/// Данные для запроса значений из INOPC
/// </summary>
public class InopcRequest
{
	/// <summary>
	/// Список путей, по которым нужно получить значения
	/// </summary>
	public required string[] Tags { get; set; }
}
