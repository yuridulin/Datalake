namespace DatalakeDatabase.Exceptions;

/// <summary>
/// Ошибка при получении каких-либо данных о том, что эти данные не были найдены
/// </summary>
/// <param name="message">Сообщение после "Не найдено: "</param>
public class NotFoundException(string? message) : Exception(message)
{
	public override string ToString()
	{
		return "Не найдено: " + Message;
	}
}
