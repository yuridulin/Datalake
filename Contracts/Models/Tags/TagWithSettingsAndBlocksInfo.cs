using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Tags;

/// <summary>
/// Полная информация о теге
/// </summary>
public class TagWithSettingsAndBlocksInfo : TagWithSettingsInfo
{
	/// <summary>
	/// Список блоков, в которых используется этот тег
	/// </summary>
	[Required]
	public required List<TagBlockRelationInfo> Blocks { get; set; }
}
