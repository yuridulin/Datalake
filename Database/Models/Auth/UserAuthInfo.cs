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
	public Dictionary<Guid, AccessRule> Groups { get; set; } = [];

	/// <summary>
	/// Список всех блоков с указанием доступа к ним
	/// </summary>
	public Dictionary<int, AccessRule> Sources { get; set; } = [];

	/// <summary>
	/// Список всех блоков с указанием доступа к ним
	/// </summary>
	public Dictionary<int, AccessRule> Blocks { get; set; } = [];

	/// <summary>
	/// Список всех тегов с указанием доступа к ним
	/// </summary>
	public Dictionary<Guid, AccessRule> Tags { get; set; } = [];
}
