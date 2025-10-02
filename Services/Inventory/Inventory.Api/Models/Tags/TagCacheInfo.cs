using System.ComponentModel.DataAnnotations;

namespace Datalake.Inventory.Api.Models.Tags;

/// <summary>
/// Базовая информация о теге для кэша
/// </summary>
public class TagCacheInfo : TagSimpleInfo
{
	/// <summary>
	/// Коэффициент преобразования (соотношение новой и исходной шкал, заданных в настройках тега)
	/// </summary>
	[Required]
	public required float ScalingCoefficient { get; set; } = 1;

	/// <summary>
	/// Тег отмечен как удаленный
	/// </summary>
	[Required]
	public required bool IsDeleted { get; set; } = false;

	/// <summary>
	/// Идентификатор источника данных тега
	/// </summary>
	[Required]
	public required int SourceId { get; set; }
}
