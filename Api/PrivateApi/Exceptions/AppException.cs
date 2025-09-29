using Microsoft.Extensions.Logging;

namespace Datalake.PrivateApi.Exceptions;

/// <summary>
/// Базовая ошибка приложения
/// </summary>
/// <param name="code">Код ошибки</param>
/// <param name="message">Сообщение</param>
/// <param name="logLevel">Уровень важности ошибки</param>
public abstract class AppException(string code, string message, LogLevel logLevel = LogLevel.Error)
	: Exception(message)
{
	/// <summary>
	/// Код ошибки
	/// </summary>
	public string Code { get; } = code;

	/// <summary>
	/// Уровень важности ошибки
	/// </summary>
	public LogLevel LogLevel { get; } = logLevel;
}
