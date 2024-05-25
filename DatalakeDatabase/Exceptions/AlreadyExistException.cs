using DatalakeDatabase.Exceptions.Base;

namespace DatalakeDatabase.Exceptions;

/// <summary>
/// Ошибка при попытке добавления чего-то, что уже существует
/// </summary>
/// <param name="message">Сообщение после "Уже существует: "</param>
public class AlreadyExistException(string? message) : DatalakeException(message)
{
	public override string ToString()
	{
		return "Уже существует: " + Message;
	}
}
