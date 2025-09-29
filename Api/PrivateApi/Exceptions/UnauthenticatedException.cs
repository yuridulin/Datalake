using Microsoft.Extensions.Logging;

namespace Datalake.PrivateApi.Exceptions;

/// <summary>
/// Выполнение действия запрещено, пользователь не аутентифицирован
/// </summary>
public class UnauthenticatedException(string code, string message)
	: AppException(code, message, LogLevel.Warning)
{
}