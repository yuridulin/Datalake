using Microsoft.Extensions.Logging;

namespace Datalake.PrivateApi.Exceptions;

/// <summary>
/// Инфраструктурная ошибка
/// </summary>
public class InfrastructureException(string code, string message)
	: AppException(code, message, LogLevel.Error)
{
	public InfrastructureException(string message) : this("INFRASTRUCTURE", message) { }
}
