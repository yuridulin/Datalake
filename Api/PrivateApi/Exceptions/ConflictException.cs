using Microsoft.Extensions.Logging;

namespace Datalake.PrivateApi.Exceptions;

/// <summary>
/// Действие не применимо или противоречиво
/// </summary>
public class ConflictException(string code, string message) : AppException(code, message, LogLevel.Warning)
{
	public ConflictException(string message) : this("CONFLICT", message) { }
}
