using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Blocks;

/// <summary>
/// Информация о блоке
/// </summary>
public class BlockWithTagsInfo : BlockSimpleInfo
{
	/// <summary>
	/// Идентификатор родительского блока
	/// </summary>
	public int? ParentId { get; set; }

	/// <summary>
	/// Текстовое описание
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Уровень доступа к блоку
	/// </summary>
	[Required]
	public AccessRuleInfo AccessRule { get; set; } = AccessRuleInfo.Default;

	/// <summary>
	/// Список прикреплённых тегов
	/// </summary>
	[Required]
	public BlockNestedTagInfo[] Tags { get; set; } = [];
}
