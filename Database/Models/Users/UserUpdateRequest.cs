using Datalake.Database.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Users;

/// <summary>
/// Данные запроса для изменения учетной записи
/// </summary>
public class UserUpdateRequest
{
	/// <summary>
	/// Новое имя для входа
	/// </summary>
	public string? Login { get; set; }

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

	/// <summary>
	/// Идентификатор пользователя в сервере EnergoId
	/// </summary>
	public Guid? EnergoIdGuid { get; set; }

	/// <summary>
	/// Тип учетной записи
	/// </summary>
	[Required]
	public UserType Type { get; set; }
}
