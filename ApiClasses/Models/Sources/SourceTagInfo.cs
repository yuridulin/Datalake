using Datalake.ApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Sources;

/// <summary>
/// Информация о теге, берущем данные из этого источника
/// </summary>
public class SourceTagInfo
{
	/// <summary>
	/// Идентификатор тега
	/// </summary>
	[Required]
	public Guid Guid { get; set; }

	/// <summary>
	/// Глобальное наименование тега
	/// </summary>
	[Required]
	public required string Name { get; set; }

	/// <summary>
	/// Путь к данным в источнике
	/// </summary>
	[Required]
	public required string Item { get; set; }

	/// <summary>
	/// Тип данных тега
	/// </summary>
	[Required]
	public TagType Type { get; set; }
}
