using DatalakeApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Sources;

/// <summary>
/// Информация о удалённой записи с данными источника
/// </summary>
public class SourceItemInfo
{
	/// <summary>
	/// Путь к данным в источнике
	/// </summary>
	[Required]
	public required string Path { get; set; }

	/// <summary>
	/// Тип данных
	/// </summary>
	[Required]
	public TagType Type { get; set; }
}
