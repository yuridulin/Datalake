namespace Datalake.Contracts.Models.Blocks;

/// <summary>
/// Информация о блоке в иерархическом представлении
/// </summary>
public class BlockTreeInfo : BlockSimpleInfo
{
	/// <summary>
	/// Идентификатор родительского блока
	/// </summary>
	public int? ParentBlockId { get; set; }
}
