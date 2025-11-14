using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Tags;

/// <summary>
/// Соответствие входного и выходного значения по таблице пороговых уставок
/// </summary>
public class TagThresholdInfo
{
	/// <summary>
	/// Пороговое значение
	/// </summary>
	[Required]
	public float Threshold { get; set; }

	/// <summary>
	/// Итоговое значение
	/// </summary>
	[Required]
	public float Result { get; set; }
}