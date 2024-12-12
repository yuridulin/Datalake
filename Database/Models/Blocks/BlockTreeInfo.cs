using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Blocks;

/// <summary>
/// Информация о блоке в иерархическом представлении
/// </summary>
public class BlockTreeInfo : BlockWithTagsInfo
{
	/// <summary>
	/// Вложенные блоки
	/// </summary>
	[Required]
	public BlockTreeInfo[] Children { get; set; } = [];

	/// <summary>
	/// Полное имя блока, включающее имена всех родительских блоков по иерархии через "."
	/// </summary>
	[Required]
	public string FullName { get; set; } = string.Empty;
}
