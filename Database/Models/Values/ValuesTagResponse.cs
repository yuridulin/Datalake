using Datalake.Database.Enums;
using Datalake.Database.Models.Tags;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Values;

/// <summary>
/// Ответ на запрос для получения значений, характеризующий запрошенный тег и его значения
/// </summary>
public class ValuesTagResponse : TagSimpleInfo
{
	/// <summary>
	/// Список значений
	/// </summary>
	[Required]
	public required ValueRecord[] Values { get; set; } = [];

	/// <summary>
	/// Флаг, говорящий о недостаточности доступа для записи у пользователя
	/// </summary>
	public bool? NoAccess { get; set; } = null;
}
