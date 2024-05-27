using DatalakeApiClasses.Exceptions.Base;

namespace DatalakeApiClasses.Exceptions;

/// <summary>
/// Ошибка при выполнении какой-либо операции с базой данных, связанная с выполнением запросов
/// </summary>
/// <param name="message">Сообщение после "Ошибка базы данных: "</param>
public class DatabaseException(string? message) : DatalakeException(message)
{
	public override string ToString()
	{
		return "Ошибка базы данных: " + Message;
	}
}
