using System.ComponentModel.DataAnnotations;

namespace Datalake.PublicApi.Models.Users;

/// <summary>
/// Информация о пользователе, взятая из Keycloak
/// </summary>
public class UserEnergoIdInfo
{
	/// <summary>
	/// Идентификатор пользователя в сервере Keycloak
	/// </summary>
	[Required]
	public required Guid EnergoIdGuid { get; set; }

	/// <summary>
	/// Идентификатор сопоставленного пользователя приложения, если есть
	/// </summary>
	public Guid? UserGuid { get; set; }

	/// <summary>
	/// Имя для входа
	/// </summary>
	[Required]
	public required string Email { get; set; }

	/// <summary>
	/// Полное имя пользователя
	/// </summary>
	[Required]
	public required string FullName { get; set; }
}
