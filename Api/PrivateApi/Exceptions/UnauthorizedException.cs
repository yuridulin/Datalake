using Microsoft.Extensions.Logging;

namespace Datalake.PrivateApi.Exceptions;

/// <summary>
/// Выполнение действия запрещено, не хватает прав доступа
/// </summary>
public abstract class UnauthorizedException(string code, string message)
	: AppException(code, message, LogLevel.Warning)
{
}
