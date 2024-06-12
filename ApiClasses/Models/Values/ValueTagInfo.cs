using Datalake.ApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Values;

/// <summary>
/// Краткая информация о запрашиваемом теге
/// </summary>
public class ValueTagInfo
{
	/// <summary>
	/// Имя тега
	/// </summary>
	[Required]
	public string TagName { get; set; } = string.Empty;

	/// <summary>
	/// Тип данных значений тега
	/// </summary>
	[Required]
	public TagType TagType { get; set; }
}
