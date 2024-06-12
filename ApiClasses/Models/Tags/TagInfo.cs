using Datalake.ApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Tags;

/// <summary>
/// Информация о теге
/// </summary>
public class TagInfo
{
	/// <summary>
	/// Идентификатор тега в локальной базе
	/// </summary>
	[Required]
	public required Guid Guid { get; set; }

	/// <summary>
	/// Имя тега
	/// </summary>
	[Required]
	public required string Name { get; set; }

	/// <summary>
	/// Произвольное описание тега
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Тип данных тега
	/// </summary>
	[Required]
	public required TagType Type { get; set; }

	/// <summary>
	/// Интервал опроса источника для получения нового значения
	/// </summary>
	[Required]
	public required short IntervalInSeconds { get; set; }

	/// <summary>
	/// Идентификатор источника данных
	/// </summary>
	[Required]
	public required int SourceId { get; set; }

	/// <summary>
	/// Тип данных источника
	/// </summary>
	[Required]
	public required SourceType SourceType { get; set; } = SourceType.Datalake;

	/// <summary>
	/// Путь к данным в источнике
	/// </summary>
	public string? SourceItem { get; set; }

	/// <summary>
	/// Имя используемого источника данных
	/// </summary>
	public string? SourceName { get; set; } = string.Empty;

	/// <summary>
	/// Формула, на основе которой вычисляется значение
	/// </summary>
	public string? Formula { get; set; } = string.Empty;

	/// <summary>
	/// Применяется ли приведение числового значения тега к другой шкале
	/// </summary>
	[Required]
	public required bool IsScaling { get; set; }

	/// <summary>
	/// Меньший предел итоговой шкалы
	/// </summary>
	[Required]
	public required float MinEu { get; set; }

	/// <summary>
	/// Больший предел итоговой шкалы
	/// </summary>
	[Required]
	public required float MaxEu { get; set; }

	/// <summary>
	/// Меньший предел шкалы исходного значения
	/// </summary>
	[Required]
	public required float MinRaw { get; set; }

	/// <summary>
	/// Больший предел шкалы исходного значения
	/// </summary>
	[Required]
	public required float MaxRaw { get; set; }

	/// <summary>
	/// Входные переменные для формулы, по которой рассчитывается значение
	/// </summary>
	[Required]
	public required TagInputInfo[] FormulaInputs { get; set; } = [];
}
