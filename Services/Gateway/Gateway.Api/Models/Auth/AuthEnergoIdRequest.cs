using System.ComponentModel.DataAnnotations;

namespace Datalake.Gateway.Api.Models.Auth;

/// <summary>
/// Информация о пользователе, взятая из Keycloak
/// </summary>
public record AuthEnergoIdRequest
{
	/// <summary>
	/// Идентификатор пользователя в сервере Keycloak
	/// </summary>
	[Required]
	public required Guid EnergoIdGuid { get; init; }

	/// <summary>
	/// Имя для входа
	/// </summary>
	[Required]
	public required string Email { get; init; }

	/// <summary>
	/// Полное имя пользователя
	/// </summary>
	[Required]
	public required string FullName { get; init; }
}
