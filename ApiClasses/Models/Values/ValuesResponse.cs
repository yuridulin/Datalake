using Datalake.ApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Values;

/// <summary>
/// Ответ на запрос для получения значений, характеризующий запрошенный тег и его значения
/// </summary>
public class ValuesResponse
{
	/// <summary>
	/// Идентификатор тега в локальной базе
	/// </summary>
	[Required]
	public required int Id { get; set; }

	/// <summary>
	/// Имя тега
	/// </summary>
	[Required]
	public required string TagName { get; set; }

	/// <summary>
	/// Тип данных
	/// </summary>
	[Required]
	public required TagType Type { get; set; }

	/// <summary>
	/// Применённый тип агрегирования
	/// </summary>
	[Required]
	public required AggregationFunc Func { get; set; }

	/// <summary>
	/// Список значений
	/// </summary>
	[Required]
	public required ValueRecord[] Values { get; set; } = [];
}
