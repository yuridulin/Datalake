using Microsoft.Extensions.Logging;

namespace Datalake.PrivateApi.Exceptions;

/// <summary>
/// Доменная ошибка, возникает при изменении данных в модели
/// </summary>
public class DomainException(string code, string message)
	: AppException(code, message, LogLevel.Warning)
{
}
