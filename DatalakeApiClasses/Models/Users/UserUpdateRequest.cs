using DatalakeApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Users;

/// <summary>
/// Данные запроса для изменения учетной записи
/// </summary>
public class UserUpdateRequest
{
	/// <summary>
	/// Новое имя для входа
	/// </summary>
	[Required]
	public required string Login { get; set; }

	/// <summary>
	/// Новый адрес статической точки, из которой будет разрешен доступ
	/// </summary>
	public string? StaticHost { get; set; }

	/// <summary>
	/// Новый пароль
	/// </summary>
	public string? Password { get; set; }

	/// <summary>
	/// Новое полное имя
	/// </summary>
	public string? FullName { get; set; }

	/// <summary>
	/// Новый глобальный уровень доступа
	/// </summary>
	[Required]
	public AccessType AccessType { get; set; }

	/// <summary>
	/// Нужно ли создать новый ключ для статичной учетной записи
	/// </summary>
	[Required]
	public bool CreateNewStaticHash { get; set; } = false;
}
