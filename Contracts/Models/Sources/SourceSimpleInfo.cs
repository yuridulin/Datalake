using Datalake.Contracts.Interfaces;
using Datalake.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Sources;

/// <summary>
/// Базовая информация о источнике, достаточная, чтобы на него сослаться
/// </summary>
public class SourceSimpleInfo : IProtectedEntity
{
	/// <summary>
	/// Идентификатор источника в базе данных
	/// </summary>
	[Required]
	public int Id { get; set; }

	/// <summary>
	/// Название источника
	/// </summary>
	[Required]
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Произвольное описание источника
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Тип протокола, по которому запрашиваются данные
	/// </summary>
	[Required]
	public SourceType Type { get; set; }

	/// <summary>
	/// Правило доступа к этому источнику данных
	/// </summary>
	[Required]
	public AccessRuleInfo AccessRule { get; set; } = AccessRuleInfo.Default;
}
