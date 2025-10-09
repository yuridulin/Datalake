using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Extensions;
using Datalake.Domain.Exceptions;
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
		if (expirationTime != null && IsExpire(expirationTime.Value))
			throw new DomainException("Дата истекания сессии не может быть в прошлом");

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

	private static bool IsExpire(DateTime date)
	{
		return DateTimeExtension.GetCurrentDateTime() <= date;
	}

	public void Validate()
	{
		if (IsExpire())
			throw new DomainException("Сессия истекла");
	}

	public void Refresh(TimeSpan timeSpan)
	{
		ExpirationTime.Add(timeSpan);
	}

	public bool IsExpire()
	{
		return IsExpire(ExpirationTime);
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

