using Microsoft.Extensions.Logging;

namespace Datalake.PrivateApi.Exceptions;

/// <summary>
/// Действие не применимо или противоречиво
/// </summary>
public class ConfictException(string code, string message)
	: AppException(code, message, LogLevel.Warning)
{
}
