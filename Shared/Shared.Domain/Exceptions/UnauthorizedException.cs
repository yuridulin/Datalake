using Datalake.Contracts.Public.Exceptions;
using Microsoft.Extensions.Logging;

namespace Datalake.Shared.Domain.Exceptions;

/// <summary>
/// Выполнение действия запрещено, не хватает прав доступа
/// </summary>
public class UnauthorizedException(string code, string message) : AppException(code, message, LogLevel.Warning)
{
	/// <summary>
	/// Доменная ошибка, возникает при изменении данных в модели
	/// </summary>
	/// <param name="message">Сообщение</param>
	public UnauthorizedException(string message) : this("NOT AUTHORIZED", message) { }
}
