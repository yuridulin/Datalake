namespace DatalakeDatabase.Exceptions;

/// <summary>
/// Ошибка при попытке добавления чего-то, что уже существует
/// </summary>
/// <param name="message">Сообщение после "Уже существует: "</param>
public class AlreadyExistException(string? message) : Exception(message)
{
	public override string ToString()
	{
		return "Уже существует: " + Message;
	}
}
