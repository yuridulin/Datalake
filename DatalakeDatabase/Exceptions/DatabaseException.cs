namespace DatalakeDatabase.Exceptions;

/// <summary>
/// Ошибка при выполнении какой-либо операции с базой данных, связанная с выполнением запросов
/// </summary>
/// <param name="message">Сообщение после "Ошибка базы данных: "</param>
public class DatabaseException(string? message) : Exception(message)
{
	public override string ToString()
	{
		return "Ошибка базы данных: " + Message;
	}
}
