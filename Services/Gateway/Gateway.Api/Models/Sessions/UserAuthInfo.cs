using Datalake.Contracts.Public.Models;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Gateway.Api.Models.Sessions;

/// <summary>
/// Информация о аутентифицированном пользователе
/// </summary>
public class UserAuthInfo
{
	/// <summary>
	/// Идентификатор пользователя
	/// </summary>
	[Required]
	public required Guid Guid { get; set; }

	/// <summary>
	/// Идентификатор пользователя в системе EnergoId
	/// </summary>
	public Guid? EnergoId { get; set; }

	/// <summary>
	/// Имя пользователя
	/// </summary>
	[Required]
	public required string FullName { get; set; }

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
