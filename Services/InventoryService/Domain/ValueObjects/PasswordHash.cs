using Datalake.PrivateApi.Exceptions;
using System.Security.Cryptography;
using System.Text;

namespace Datalake.InventoryService.Domain.ValueObjects;

/// <summary>
/// Value Object для хэшированного пароля
/// </summary>
public sealed class PasswordHash : IEquatable<PasswordHash>
{
	// Приватный конструктор - только через фабричные методы
	private PasswordHash(string? hash)
	{
		_hash = hash ?? throw new DomainException("Нельзя создать без хэша");
	}

	private readonly string _hash;

	public string Value => _hash;

	/// <summary>
	/// Пустой пароль (для статических пользователей)
	/// </summary>
	public static PasswordHash FromEmpty()
	{
		using var rng = RandomNumberGenerator.Create();
		var randomNumber = new byte[32];
		rng.GetBytes(randomNumber);
		return new PasswordHash(Convert.ToBase64String(randomNumber));
	}

	/// <summary>
	/// Создать из plain text
	/// </summary>
	public static PasswordHash FromPlainText(string? plainText)
	{
		if (string.IsNullOrEmpty(plainText))
			throw new DomainException("Пароль не может быть пустым");

		var hash = SHA1.HashData(Encoding.UTF8.GetBytes(plainText));
		return new PasswordHash(Convert.ToBase64String(hash));
	}

	/// <summary>
	/// Восстановить из существующего хэша (при загрузке из БД)
	/// </summary>
	public static PasswordHash FromExistingHash(string hash)
	{
		if (string.IsNullOrEmpty(hash))
			throw new DomainException("Хэш не может быть пустым");

		return new PasswordHash(hash);
	}

	// Value Object семантика
	public bool Equals(PasswordHash? other)
	{
		if (other is null)
			return false;
		return _hash == other._hash;
	}

	public override bool Equals(object? obj)
			=> Equals(obj as PasswordHash);

	public override int GetHashCode()
			=> _hash.GetHashCode();

	public static bool operator ==(PasswordHash? left, PasswordHash? right)
			=> Equals(left, right);

	public static bool operator !=(PasswordHash? left, PasswordHash? right)
			=> !Equals(left, right);

	/// <summary>
	/// Для сохранения в БД
	/// </summary>
	public override string ToString() => _hash;

	/// <summary>
	/// Проверка пароля (можно вынести в отдельный сервис, если нужна сложная логика)
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
