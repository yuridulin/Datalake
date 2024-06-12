using DatalakeApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Values;

/// <summary>
/// Краткая информация о запрашиваемом теге
/// </summary>
public class ValueTagInfo
{
	/// <summary>
	/// Глобальный идентификатор тега
	/// </summary>
	[Required]
	public required Guid Guid { get; set; }

	/// <summary>
	/// Имя тега
	/// </summary>
	[Required]
	public required string TagName { get; set; }

	/// <summary>
	/// Тип данных значений тега
	/// </summary>
	[Required]
	public TagType TagType { get; set; }
}
