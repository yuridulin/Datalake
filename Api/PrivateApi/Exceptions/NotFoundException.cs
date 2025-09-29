using Microsoft.Extensions.Logging;

namespace Datalake.PrivateApi.Exceptions;

/// <summary>
/// Ошибка при получении каких-либо данных о том, что эти данные не были найдены
/// </summary>
public class NotFoundException(string code, string message)
	: AppException(code, message, LogLevel.Warning)
{
}
