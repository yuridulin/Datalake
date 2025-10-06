using Datalake.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Datalake.Shared.Application.Exceptions;

/// <summary>
/// Выполнение действия запрещено, пользователь не аутентифицирован
/// </summary>
public class UnauthenticatedException(string code, string message)
	: AppException(code, message, LogLevel.Warning)
{
	/// <summary>
	/// Выполнение действия запрещено, пользователь не аутентифицирован
	/// </summary>
	/// <param name="message">Сообщение</param>
	public UnauthenticatedException(string message) : this("NOT_AUTHENTICATED", message) { }
}