using DatalakeApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Users;

/// <summary>
/// Данные запроса на создание пользователя
/// </summary>
public class UserCreateRequest
{
	/// <summary>
	/// Имя для входа
	/// </summary>
	[Required]
	public required string Login { get; set; }

	/// <summary>
	/// Полное имя пользователя
	/// </summary>
	public string? FullName { get; set; }

	/// <summary>
	/// Используемый пароль
	/// </summary>
	public string? Password { get; set; }

	/// <summary>
	/// Адрес статической точки, откуда будет осуществляться доступ
	/// </summary>
	public string? StaticHost { get; set; }

	/// <summary>
	/// Глобальный уровень доступа
	/// </summary>
	[Required]
	public AccessType AccessType { get; set; }
}
