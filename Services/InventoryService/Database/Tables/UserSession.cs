using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Database.Tables;

/// <summary>
/// Запись о сессии учетной записи
/// </summary>
public record class UserSession
{
	private UserSession() { }

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	public Guid UserGuid { get; set; }

	/// <summary>
	/// Дата создания сессии
	/// </summary>
	public DateTime Created { get; set; }

	/// <summary>
	/// Время истечения сессии
	/// </summary>
	public DateTime ExpirationTime { get; set; }

	/// <summary>
	/// Токен доступа
	/// </summary>
	public string Token { get; set; } = null!;

	/// <summary>
	/// Тип входа в сессию. Нужен, чтобы правильно выйти
	/// </summary>
	public UserType Type { get; set; }

	// связи

	/// <summary>
	/// Связанная учетная запись
	/// </summary>
	public User User { get; set; } = null!;
}
