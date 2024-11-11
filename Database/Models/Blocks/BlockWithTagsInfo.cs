using Datalake.Database.Models.Auth;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Blocks;

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
	public IEnumerable<BlockNestedTagInfo> Tags { get; set; } = [];
}
