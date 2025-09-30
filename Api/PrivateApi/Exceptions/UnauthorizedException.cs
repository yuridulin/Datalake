using Microsoft.Extensions.Logging;

namespace Datalake.PrivateApi.Exceptions;

/// <summary>
/// Выполнение действия запрещено, не хватает прав доступа
/// </summary>
public class UnauthorizedException(string code, string message)
	: AppException(code, message, LogLevel.Warning)
{
	public UnauthorizedException(string message) : this("NOT_AUTHORIZED", message) { }
}
