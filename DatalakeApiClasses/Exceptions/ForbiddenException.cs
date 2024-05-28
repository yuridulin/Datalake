using DatalakeApiClasses.Exceptions.Base;

namespace DatalakeApiClasses.Exceptions;

/// <summary>
/// Ошибка при попытке выполнить действие, выполнение которого защищено по соображениям безопасности
/// </summary>
/// <param name="message">Сообщение после "Запрещено: "</param>
public class ForbiddenException(string? message) : DatalakeException(message)
{
	public override string ToString()
	{
		return "Запрещено: " + Message;
	}
}
