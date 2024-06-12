using Datalake.ApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Users;

/// <summary>
/// Данные запроса на создание пользователя
/// </summary>
public class UserCreateRequest
{
	/// <summary>
	/// Имя для входа
	/// </summary>
	public string? Login { get; set; }

	/// <summary>
	/// Полное имя пользователя
	/// </summary>
	public string? FullName { get; set; }

	/// <summary>
	/// Глобальный уровень доступа
	/// </summary>
	[Required]
	public required AccessType AccessType { get; set; }

	/// <summary>
	/// Тип учетной записи
	/// </summary>
	[Required]
	public required UserType Type { get; set; }

	/// <summary>
	/// Используемый пароль
	/// </summary>
	public string? Password { get; set; }

	/// <summary>
	/// Адрес статической точки, откуда будет осуществляться доступ
	/// </summary>
	public string? StaticHost { get; set; }

	/// <summary>
	/// Идентификатор связанной учетной записи в сервере EnergoId
	/// </summary>
	public Guid? EnergoIdGuid { get; set; }
}
