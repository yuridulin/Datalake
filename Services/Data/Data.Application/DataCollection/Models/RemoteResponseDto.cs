namespace Datalake.Data.Application.DataCollection.Models;

/// <summary>
/// Ответ после получения данных с источника
/// </summary>
public class RemoteResponseDto
{
	/// <summary>
	/// Наличие связи с источником
	/// </summary>
	public required bool IsConnected { get; set; }

	/// <summary>
	/// Дата получения данных
	/// </summary>
	public DateTime Timestamp { get; set; }

	/// <summary>
	/// Список полученных значений
	/// </summary>
	public RemoteRequestItemDto[] Tags { get; set; } = [];
}
