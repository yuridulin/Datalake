using Datalake.Contracts.Public.Exceptions;
using Microsoft.Extensions.Logging;

namespace Datalake.Shared.Domain.Exceptions;

/// <summary>
/// Доменная ошибка, возникает при изменении данных в модели
/// </summary>
public class DomainException(string code, string message) : AppException(code, message, LogLevel.Warning)
{
	/// <summary>
	/// Доменная ошибка, возникает при изменении данных в модели
	/// </summary>
	/// <param name="message">Сообщение</param>
	public DomainException(string message) : this("DOMAIN", message) { }
}
