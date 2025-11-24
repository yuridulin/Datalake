using Datalake.Contracts.Models.AccessRules;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Blocks;

/// <summary>
/// Информация о блоке
/// </summary>
public record BlockDetailedInfo : BlockWithTagsInfo
{
	/// <summary>
	/// Список дочерних блоков
	/// </summary>
	[Required]
	public BlockSimpleInfo[] Children { get; set; } = [];

	/// <summary>
	/// Список родительских блоков
	/// </summary>
	[Required]
	public BlockSimpleInfo[] Adults { get; set; } = [];

	/// <summary>
	/// Список статических свойств блока
	/// </summary>
	[Required]
	public BlockPropertyInfo[] Properties { get; set; } = [];

	/// <summary>
	/// Список правил прав доступа, которые напрямую связаны с блоком
	/// </summary>
	[Required]
	public AccessRulesForObjectInfo[] AccessRules { get; set; } = [];
}
