namespace DatalakeDatabase.Exceptions;

/// <summary>
/// Ошибка при попытке выполнить действие, выполнение которого защищено по соображениям безопасности
/// </summary>
/// <param name="message">Сообщение после "Запрещено: "</param>
public class ForbiddenException(string? message) : Exception(message)
{
	public override string ToString()
	{
		return "Запрещено: " + Message;
	}
}
