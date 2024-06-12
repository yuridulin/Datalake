namespace Datalake.ApiClasses.Exceptions.Base;

/// <summary>
/// Базовый класс ошибок приложения
/// </summary>
/// <param name="message">Сообщение о произошедшем событии</param>
public abstract class DatalakeException(string? message) : Exception(message)
{
}
