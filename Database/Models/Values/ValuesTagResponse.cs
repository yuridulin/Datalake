using Datalake.Database.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Values;

/// <summary>
/// Ответ на запрос для получения значений, характеризующий запрошенный тег и его значения
/// </summary>
public class ValuesTagResponse
{
	/// <summary>
	/// Идентификатор тега в локальной базе
	/// </summary>
	[Required]
	public required int Id { get; set; }

	/// <summary>
	/// Глобальный идентификатор тега
	/// </summary>
	[Required]
	public required Guid Guid { get; set; }

	/// <summary>
	/// Полное наименование тега
	/// </summary>
	[Required]
	public required string Name { get; set; }

	/// <summary>
	/// Тип данных
	/// </summary>
	[Required]
	public required TagType Type { get; set; }

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
