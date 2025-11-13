using Datalake.Contracts.Public.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Public.Models.Users;

/// <summary>
/// Данные запроса на создание пользователя
/// </summary>
public class UserCreateRequest
{
	/// <summary>
	/// Тип учетной записи
	/// </summary>
	[Required]
	public required UserType Type { get; set; }

	/// <summary>
	/// Идентификатор связанной учетной записи в сервере EnergoId
	/// </summary>
	public Guid? EnergoIdGuid { get; set; }

	/// <summary>
	/// Имя для входа
	/// </summary>
	public string? Login { get; set; }

	/// <summary>
	/// Используемый пароль
	/// </summary>
	public string? Password { get; set; }

	/// <summary>
	/// Полное имя пользователя
	/// </summary>
	public string? FullName { get; set; }

	/// <summary>
	/// Почтовый адрес
	/// </summary>
	public string? Email { get; set; }

	/// <summary>
	/// Глобальный уровень доступа
	/// </summary>
	[Required]
	public required AccessType AccessType { get; set; }
}
