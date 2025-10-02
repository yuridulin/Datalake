using Datalake.Contracts.Public.Exceptions;
using Microsoft.Extensions.Logging;

namespace Datalake.Shared.Application.Exceptions;

/// <summary>
/// Инфраструктурная ошибка
/// </summary>
public class InfrastructureException(string code, string message)
	: AppException(code, message, LogLevel.Error)
{
	/// <summary>
	/// Инфраструктурная ошибка
	/// </summary>
	/// <param name="message">Сообщение</param>
	public InfrastructureException(string message) : this("INFRASTRUCTURE", message) { }
}
