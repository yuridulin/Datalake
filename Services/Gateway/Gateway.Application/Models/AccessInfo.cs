using Datalake.Contracts.Models;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Gateway.Application.Models;

/// <summary>
/// Информация о аутентифицированном пользователе
/// </summary>
public class AccessInfo
{
	/// <summary>
	/// Глобальный уровень доступа
	/// </summary>
	[Required]
	public required AccessRuleInfo RootRule { get; set; }

	/// <summary>
	/// Список всех блоков с указанием доступа к ним
	/// </summary>
	[Required]
	public Dictionary<Guid, AccessRuleInfo> Groups { get; set; } = [];

	/// <summary>
	/// Список всех блоков с указанием доступа к ним
	/// </summary>
	[Required]
	public Dictionary<int, AccessRuleInfo> Sources { get; set; } = [];

	/// <summary>
	/// Список всех блоков с указанием доступа к ним
	/// </summary>
	[Required]
	public Dictionary<int, AccessRuleInfo> Blocks { get; set; } = [];

	/// <summary>
	/// Список всех тегов с указанием доступа к ним
	/// </summary>
	[Required]
	public Dictionary<int, AccessRuleInfo> Tags { get; set; } = [];
}
