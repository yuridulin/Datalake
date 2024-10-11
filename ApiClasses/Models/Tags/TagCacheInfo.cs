using Datalake.ApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Tags;

/// <summary>
/// Базовая информация о теге для кэша
/// </summary>
public class TagCacheInfo : TagSimpleInfo
{
	/// <summary>
	/// Тип значений
	/// </summary>
	[Required]
	public required TagType TagType { get; set; }

	/// <summary>
	/// Тип источника значений
	/// </summary>
	[Required]
	public required SourceType SourceType { get; set; }

	/// <summary>
	/// Является ли тег мануальным - влияет на метод записи
	/// </summary>
	[Required]
	public required bool IsManual { get; set; }

	/// <summary>
	/// Коэффициент преобразования (соотношение новой и исходной шкал, заданных в настройках тега)
	/// </summary>
	[Required]
	public required float ScalingCoefficient { get; set; } = 1;
}
