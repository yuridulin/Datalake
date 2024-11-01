using Datalake.Database.Enums;
using Datalake.Database.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Auth;

/// <summary>
/// Информация о аутентифицированном пользователе
/// </summary>
public class UserAuthInfo : UserSimpleInfo
{
	/// <summary>
	/// Идентификатор сессии
	/// </summary>
	[Required]
	public required string Token { get; set; }
	/// <summary>
	/// Глобальный уровень доступа
	/// </summary>
	[Required]
	public required AccessType GlobalAccessType { get; set; }

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
	public Dictionary<Guid, AccessRuleInfo> Tags { get; set; } = [];

	/// <summary>
	/// Идентификатор пользователя внешнего приложения, который передается через промежуточную учетную запись
	/// </summary>
	public Guid? UnderlyingUserGuid { get; set; } = null;
}
