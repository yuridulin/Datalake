using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Users;

/// <summary>
/// Информация о пользователе, взятая из Keycloak
/// </summary>
public class UserKeycloakInfo
{
	/// <summary>
	/// Идентификатор пользователя в сервере Keycloak
	/// </summary>
	[Required]
	public required Guid KeycloakGuid { get; set; }

	/// <summary>
	/// Имя для входа
	/// </summary>
	[Required]
	public required string Login { get; set; }

	/// <summary>
	/// Полное имя пользователя
	/// </summary>
	[Required]
	public required string FullName { get; set; }
}
