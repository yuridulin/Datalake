using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Blocks;

/// <summary>
/// Информация о сущности в иерархическом представлении
/// </summary>
public class BlockTreeInfo : BlockSimpleInfo
{
	/// <summary>
	/// Вложенные сущности, подчинённые этой
	/// </summary>
	[Required]
	public BlockTreeInfo[] Children { get; set; } = [];
}
