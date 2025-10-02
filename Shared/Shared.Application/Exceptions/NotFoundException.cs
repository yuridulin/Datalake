using Datalake.Contracts.Public.Exceptions;
using Microsoft.Extensions.Logging;

namespace Datalake.Shared.Application.Exceptions;

/// <summary>
/// Ошибка при получении каких-либо данных о том, что эти данные не были найдены
/// </summary>
public class NotFoundException(string code, string message) : AppException(code, message, LogLevel.Warning)
{
	/// <summary>
	/// Ошибка при получении каких-либо данных о том, что эти данные не были найдены
	/// </summary>
	/// <param name="message">Сообщение</param>
	public NotFoundException(string message) : this("NOT_FOUND", message) { }
}
