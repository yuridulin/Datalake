using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Sources;

/// <summary>
/// Расширенная информация о источнике, включающая список связанных тегов
/// </summary>
public class SourceWithSettingsAndTagsInfo : SourceWithSettingsInfo
{
	/// <summary>
	/// Список тегов, которые получают данные из этого источника
	/// </summary>
	[Required]
	public List<SourceTagInfo> Tags { get; set; } = [];
}
