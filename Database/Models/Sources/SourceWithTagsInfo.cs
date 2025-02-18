using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Sources;

/// <summary>
/// Расширенная информация о источнике, включающая список связанных тегов
/// </summary>
public class SourceWithTagsInfo : SourceInfo
{
	/// <summary>
	/// Список тегов, которые получают данные из этого источника
	/// </summary>
	[Required]
	public SourceTagInfo[] Tags { get; set; } = [];
}
