using Datalake.PublicApi.Exceptions.Base;

namespace Datalake.PublicApi.Exceptions;

/// <summary>
/// Ошибка при аутентификации пользователя
/// </summary>
public class UnauthenticatedException(string? message) : DatalakeException(message)
{
	/// <summary>
	/// Генерация строкового сообщения о ошибке
	/// </summary>
	/// <returns>Сообщение об ошибке</returns>
	public override string ToString()
	{
		return Message;
	}
}
