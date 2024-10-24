namespace Datalake.Database.Exceptions.Base;

/// <summary>
/// Базовый класс ошибок приложения
/// </summary>
/// <param name="message">Сообщение о произошедшем событии</param>
/// <param name="innerException">Внутренняя ошибка</param>
public abstract class DatalakeException(string? message, Exception? innerException = null) : Exception(message, innerException)
{
}
