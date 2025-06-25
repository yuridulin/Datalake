namespace Datalake.Server.Services.Auth.Models;

/// <summary>
/// Запись о пользователе, полученная из сервера EnergoID по публичному API
/// </summary>
public class EnergoIdUserData
{
	/// <summary>
	/// GUID идентификатор пользователя
	/// </summary>
	public required string Sid { get; set; }

	/// <summary>
	/// Полное имя пользователя, как правило Ф.И.О.
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Электронный адрес пользователя
	/// </summary>
	public required string Email { get; set; }
}
