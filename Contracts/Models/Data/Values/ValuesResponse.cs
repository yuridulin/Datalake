using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Data.Values;

/// <summary>
/// Ответ на запрос для получения значений, включающий обработанные теги и идентификатор запроса
/// </summary>
public class ValuesResponse
{
	/// <summary>
	/// Идентификатор запроса, который будет передан в соответствующий объект ответа
	/// </summary>
	[Required]
	public required string RequestKey { get; set; }

	/// <summary>
	/// Список глобальных идентификаторов тегов
	/// </summary>
	[Required]
	public required List<ValuesTagResponse> Tags { get; set; } = [];
}
