using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Extensions;
using Datalake.Domain.ValueObjects;

namespace Datalake.Domain.Entities;

/// <summary>
/// Запись о сессии учетной записи
/// </summary>
public record class UserSession
{
	private UserSession() { }

	/// <summary>
	/// Создание новой сессии
	/// </summary>
	/// <param name="userGuid"></param>
	/// <param name="type"></param>
	/// <param name="expirationTime"></param>
	public static UserSession Create(Guid userGuid, UserType type, DateTime? expirationTime = null)
	{
		var now = DateTimeExtension.GetCurrentDateTime();
		return new()
		{
			UserGuid = userGuid,
			Type = type,
			Token = PasswordHashValue.FromEmpty(),
			Created = now,
			ExpirationTime = expirationTime ?? now.AddDays(7),
		};
	}

	public int Id { get; private set; }

	/// <summary>
	/// Идентификатор
	/// </summary>
	public Guid UserGuid { get; private set; }

	/// <summary>
	/// Дата создания сессии
	/// </summary>
	public DateTime Created { get; private set; }

	/// <summary>
	/// Время истечения сессии
	/// </summary>
	public DateTime ExpirationTime { get; private set; }

	/// <summary>
	/// Токен доступа
	/// </summary>
	public PasswordHashValue Token { get; private set; } = null!;

	/// <summary>
	/// Тип входа в сессию. Нужен, чтобы правильно выйти
	/// </summary>
	public UserType Type { get; private set; }

	// связи

	/// <summary>
	/// Связанная учетная запись
	/// </summary>
	public User User { get; set; } = null!;
}

