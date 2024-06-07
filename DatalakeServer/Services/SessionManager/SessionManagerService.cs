using DatalakeApiClasses.Models.Users;
using DatalakeServer.Constants;
using DatalakeServer.Services.SessionManager.Models;

namespace DatalakeServer.Services.SessionManager;

/// <summary>
/// Менеджер сессий пользователей
/// </summary>
public class SessionManagerService
{
	/// <summary>
	/// Список текущих сессий
	/// </summary>
	List<AuthSession> Sessions { get; set; } = [];

	/// <summary>
	/// Получить текущую сессию по токену
	/// </summary>
	/// <param name="token">Токен сессии</param>
	/// <returns>Информация о сессии</returns>
	public AuthSession? GetExistSession(string token)
	{
		var session = Sessions.FirstOrDefault(x => x.User.Token == token);
		if (session == null)
			return null;
		if (session.ExpirationTime < DateTime.UtcNow)
		{
			RemoveSession(session);
			return null;
		}
		return session;
	}

	/// <summary>
	/// Получить текущую сессию по токену
	/// </summary>
	/// <param name="context">Контекст запроса</param>
	/// <returns>Информация о сессии, если она есть</returns>
	public AuthSession? GetExistSession(HttpContext context)
	{
		var token = context.Request.Headers[AuthConstants.TokenHeader];
		if (!string.IsNullOrEmpty(token))
		{
			var tokenValue = token.ToString();
			var session = GetExistSession(tokenValue);
			if (session == null)
				return null;
			AddSessionToResponse(session, context.Response);
			return session;
		}
		return null;
	}

	/// <summary>
	/// Добавление данных о сессии к запросу
	/// </summary>
	/// <param name="session">Сессия</param>
	/// <param name="response">Запрос</param>
	public void AddSessionToResponse(AuthSession session, HttpResponse response)
	{
		response.Headers[AuthConstants.TokenHeader] = session.User.Token;
		response.Headers[AuthConstants.NameHeader] = session.User.Login;
	}

	/// <summary>
	/// Создание новой сессии для пользователя
	/// </summary>
	/// <param name="userAuthInfo">Информация о пользователе</param>
	/// <returns>Информация о сессии</returns>
	public AuthSession OpenSession(UserAuthInfo userAuthInfo)
	{
		Sessions.RemoveAll(x => x.User.Login == userAuthInfo.Login);

		var session = new AuthSession
		{
			User = userAuthInfo,
			ExpirationTime = DateTime.UtcNow.AddDays(7), // срок жизни сессии
		};
		session.User.Token = new Random().Next().ToString();
		Sessions.Add(session);

		return session;
	}

	/// <summary>
	/// Закрытие сессии
	/// </summary>
	/// <param name="token">Токен сессии</param>
	public void CloseSession(string token)
	{
		var session = GetExistSession(token);
		if (session != null)
		{
			RemoveSession(session);
		}
	}

	/// <summary>
	/// Удаление сессии из списка
	/// </summary>
	/// <param name="session">Выбранная сессия</param>
	void RemoveSession(AuthSession session)
	{
		Sessions.Remove(session);
	}
}
