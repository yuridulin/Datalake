namespace Datalake.Gateway.Host.Interfaces;

/// <summary>
/// Сервис извлечения токена сессии пользователя
/// </summary>
public interface ISessionTokenExtractor
{
	/// <summary>
	/// Извлечение токена сессии пользователя
	/// </summary>
	/// <param name="request">Запрос</param>
	string ExtractToken(HttpRequest request);

	/// <summary>
	/// Извлечение токена сессии пользователя
	/// </summary>
	/// <param name="context">Запрос</param>
	string ExtractToken(HttpContext context);
}
