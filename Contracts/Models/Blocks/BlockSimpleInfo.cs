using Datalake.Contracts.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Blocks;

/// <summary>
/// Базовая информация о блоке, достаточная, чтобы на него сослаться
/// </summary>
public record BlockSimpleInfo : IProtectedEntity
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	[Required]
	public required int Id { get; set; } = 0;

	/// <summary>
	/// Идентификатор родительского блока
	/// </summary>
	public int? ParentBlockId { get; set; }

	/// <summary>
	/// Глобальный идентификатор
	/// </summary>
	[Required]
	public required Guid Guid { get; set; }

	/// <summary>
	/// Наименование
	/// </summary>
	[Required]
	public required string Name { get; set; }

	/// <summary>
	/// Текстовое описание
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Уровень доступа к информации
	/// </summary>
	[Required]
	public AccessRuleInfo AccessRule { get; set; } = AccessRuleInfo.Default;
}
