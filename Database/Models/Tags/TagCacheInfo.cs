using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Tags;

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
}
