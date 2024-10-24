using Datalake.Database.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Sources;

/// <summary>
/// Информация о источнике
/// </summary>
public class SourceInfo : SourceSimpleInfo
{
	/// <summary>
	/// Произвольное описание источника
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Используемый для получения данных адрес
	/// </summary>
	public string? Address { get; set; }

	/// <summary>
	/// Тип протокола, по которому запрашиваются данные
	/// </summary>
	[Required]
	public SourceType Type { get; set; }
}
