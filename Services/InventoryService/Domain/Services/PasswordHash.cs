using Datalake.PublicApi.Exceptions;
using System.Security.Cryptography;
using System.Text;

namespace Datalake.InventoryService.Domain.Services;

/// <summary>
/// Хэшированный пароль
/// </summary>
public static class PasswordHash
{
	public static string FromEmpty()
	{
		using var rng = RandomNumberGenerator.Create();

		var randomNumber = new byte[32];
		rng.GetBytes(randomNumber);

		return Encode(randomNumber);
	}

	public static string FromPlainText(string? rawPassword)
	{
		if (string.IsNullOrEmpty(rawPassword))
			throw new InvalidValueException(message: "пароль не может быть пустым");

		var hash = SHA1.HashData(Encoding.UTF8.GetBytes(rawPassword));
		return Encode(hash);
	}

	private static string Encode(byte[] bytes) => Convert.ToBase64String(bytes);
}
