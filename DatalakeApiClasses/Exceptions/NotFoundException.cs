using DatalakeApiClasses.Exceptions.Base;

namespace DatalakeApiClasses.Exceptions;

/// <summary>
/// Ошибка при получении каких-либо данных о том, что эти данные не были найдены
/// </summary>
/// <param name="message">Сообщение после "Не найдено: "</param>
public class NotFoundException(string? message) : DatalakeException(message)
{
	public override string ToString()
	{
		return "Не найдено: " + Message;
	}
}
