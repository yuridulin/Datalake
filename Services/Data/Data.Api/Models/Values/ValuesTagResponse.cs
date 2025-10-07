using Datalake.Contracts.Public.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Data.Api.Models.Values;

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
	/// Имя тега
	/// </summary>
	[Required]
	public required string Name { get; set; }

	/// <summary>
	/// Тип данных тега
	/// </summary>
	[Required]
	public required TagType Type { get; set; }

	/// <summary>
	/// Частота записи тега
	/// </summary>
	[Required]
	public required TagResolution Resolution { get; set; }

	/// <summary>
	/// Тип данных источника
	/// </summary>
	[Required]
	public required SourceType SourceType { get; set; } = SourceType.Datalake;

	/// <summary>
	/// Список значений
	/// </summary>
	[Required]
	public required ValueRecord[] Values { get; set; } = [];
}
