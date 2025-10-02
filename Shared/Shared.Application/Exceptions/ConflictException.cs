using Datalake.Contracts.Public.Exceptions;
using Microsoft.Extensions.Logging;

namespace Datalake.Shared.Application.Exceptions;

/// <summary>
/// Действие не применимо или противоречиво
/// </summary>
public class ConflictException(string code, string message) : AppException(code, message, LogLevel.Warning)
{
	/// <summary>
	/// Действие не применимо или противоречиво
	/// </summary>
	/// <param name="message">Сообщение</param>
	public ConflictException(string message) : this("CONFLICT", message) { }
}
