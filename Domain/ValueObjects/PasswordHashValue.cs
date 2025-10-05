using Datalake.Domain.Exceptions;
using System.Security.Cryptography;
using System.Text;

namespace Datalake.Domain.ValueObjects;

/// <summary>
/// Value Object для хэшированного пароля
/// </summary>
public sealed record PasswordHashValue
{
	private PasswordHashValue(string? hash)
	{
		_hash = hash ?? throw new DomainException("Нельзя создать без хэша");
	}

	public string Value => _hash;
	private readonly string _hash;

	/// <summary>
	/// Генерация нового случайного хэша
	/// </summary>
	public static PasswordHashValue FromEmpty()
	{
		using var rng = RandomNumberGenerator.Create();
		var randomNumber = new byte[32];
		rng.GetBytes(randomNumber);
		return new PasswordHashValue(Convert.ToBase64String(randomNumber));
	}

	/// <summary>
	/// Создать из plain text
	/// </summary>
	public static PasswordHashValue FromPlainText(string? plainText)
	{
		if (string.IsNullOrEmpty(plainText))
			throw new DomainException("Пароль не может быть пустым");

		var hash = SHA1.HashData(Encoding.UTF8.GetBytes(plainText));
		return new PasswordHashValue(Convert.ToBase64String(hash));
	}

	/// <summary>
	/// Восстановить из существующего хэша
	/// </summary>
	public static PasswordHashValue FromExistingHash(string? hash)
	{
		if (string.IsNullOrEmpty(hash))
			throw new DomainException("Хэш не может быть пустым");

		return new PasswordHashValue(hash);
	}

	/// <summary>
	/// Для сохранения в БД
	/// </summary>
	public override string ToString() => _hash;

	/// <summary>
	/// Проверка пароля
	/// </summary>
	public bool Verify(string? plainText)
	{
		if (string.IsNullOrEmpty(plainText))
			return false;

		var testHash = SHA1.HashData(Encoding.UTF8.GetBytes(plainText));
		var testHashString = Convert.ToBase64String(testHash);

		return _hash == testHashString;
	}
}
