using Datalake.ApiClasses.Exceptions.Base;

namespace Datalake.ApiClasses.Exceptions;

/// <summary>
/// Ошибка при выполнении какой-либо операции с базой данных, связанная с выполнением запросов
/// </summary>
/// <param name="message">Сообщение после "Ошибка базы данных: "</param>
public class DatabaseException(string? message) : DatalakeException(message)
{
	/// <summary>
	/// Генерация строкового сообщения о ошибке
	/// </summary>
	/// <returns>Сообщение об ошибке</returns>
	public override string ToString()
	{
		return "Ошибка базы данных: " + Message;
	}
}
