using Datalake.Database.Exceptions.Base;

namespace Datalake.Database.Exceptions;

/// <summary>
/// Ошибка при попытке добавления чего-то, что уже существует
/// </summary>
/// <param name="message">Сообщение после "Уже существует: "</param>
public class AlreadyExistException(string? message) : DatalakeException(message)
{
	/// <summary>
	/// Генерация строкового сообщения о ошибке
	/// </summary>
	/// <returns>Сообщение об ошибке</returns>
	public override string ToString()
	{
		return "Уже существует: " + Message;
	}
}
