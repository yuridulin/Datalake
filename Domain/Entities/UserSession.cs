using Datalake.Domain.Enums;
using Datalake.Domain.Exceptions;
using System.Security.Cryptography;

namespace Datalake.Domain.Entities;

/// <summary>
/// Запись о сессии учетной записи
/// </summary>
public record class UserSession
{
	#region Конструкторы

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

		var now = DateTime.UtcNow;

		return new()
		{
			UserGuid = userGuid,
			Type = type,
			Token = CreateToken(),
			Created = now,
			ExpirationTime = expirationTime ?? now.AddDays(7),
		};
	}

	#endregion Конструкторы

	#region Методы

	private static bool IsExpire(DateTime date)
	{
		return DateTime.UtcNow > date;
	}

	/// <summary>
	/// Обновление сессии на указанное количество времени
	/// </summary>
	/// <param name="timeSpan">Время</param>
	public void Refresh(TimeSpan timeSpan)
	{
		ExpirationTime += timeSpan;
	}

	/// <summary>
	/// Проверка, истекла ли сессия
	/// </summary>
	public bool IsExpire()
	{
		return IsExpire(ExpirationTime);
	}

	/// <summary>
	/// Генерация нового случайного хэша
	/// </summary>
	private static string CreateToken()
	{
		using var rng = RandomNumberGenerator.Create();
		var randomNumber = new byte[32];
		rng.GetBytes(randomNumber);
		return Convert.ToBase64String(randomNumber);
	}

	#endregion Методы

	#region Свойства

	/// <summary>
	/// Идентификатор
	/// </summary>
	public int Id { get; private set; }

	/// <summary>
	/// Идентификатор пользователя
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
	public string Token { get; private set; } = null!;

	/// <summary>
	/// Тип входа в сессию. Нужен, чтобы правильно выйти
	/// </summary>
	public UserType Type { get; private set; }

	#endregion Свойства

	#region Связи

	/// <summary>
	/// Связанная учетная запись
	/// </summary>
	public User User { get; set; } = null!;

	#endregion Связи
}

