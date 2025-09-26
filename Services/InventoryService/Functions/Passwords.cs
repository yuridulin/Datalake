using Datalake.PublicApi.Exceptions;
using System.Security.Cryptography;
using System.Text;

namespace Datalake.Inventory.Functions;

/// <summary>
/// Функции для работы с хэшами паролей
/// </summary>
public static class Passwords
{
	/// <summary>
	/// Генерация хэша по паролю
	/// </summary>
	/// <param name="password">Пароль в незашифрованом виде</param>
	/// <exception cref="InvalidValueException">Пустой пароль</exception>
	public static string GetHashFromPassword(string? password)
	{
		if (string.IsNullOrEmpty(password))
			throw new InvalidValueException(message: "пароль не может быть пустым");

		var hash = SHA1.HashData(Encoding.UTF8.GetBytes(password));
		return Convert.ToBase64String(hash);
	}

	internal static string GenerateNewHashForStatic(HashSet<string> oldHashes)
	{
		string hash;

		int countOfGenerations = 0;
		do
		{
			hash = RandomHash();
			countOfGenerations++;
		}
		while (oldHashes.Contains(hash) && countOfGenerations < 100);

		if (countOfGenerations >= 100)
		{
			throw new DatabaseException(message: "не удалось создать новый уникальный api-ключ за 100 шагов", innerException: null);
		}

		return hash;
	}

	internal static string RandomHash()
	{
		using var rng = RandomNumberGenerator.Create();

		var randomNumber = new byte[32];
		rng.GetBytes(randomNumber);

		string refreshToken = Convert.ToBase64String(randomNumber);
		return refreshToken;
	}
}
