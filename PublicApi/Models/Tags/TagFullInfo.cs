using System.ComponentModel.DataAnnotations;

namespace Datalake.PublicApi.Models.Tags;

/// <summary>
/// Полная информация о теге
/// </summary>
public class TagFullInfo : TagInfo
{
	/// <summary>
	/// Список блоков, в которых используется этот тег
	/// </summary>
	[Required]
	public required TagBlockRelationInfo[] Blocks { get; set; }
}
