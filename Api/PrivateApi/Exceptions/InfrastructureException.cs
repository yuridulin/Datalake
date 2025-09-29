using Microsoft.Extensions.Logging;

namespace Datalake.PrivateApi.Exceptions;

/// <summary>
/// Инфраструктурная ошибка
/// </summary>
public abstract class InfrastructureException(string code, string message)
	: AppException(code, message, LogLevel.Error)
{
}
