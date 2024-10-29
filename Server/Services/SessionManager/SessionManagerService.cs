using Datalake.Database.Models.Auth;
using Datalake.Server.Constants;
using Datalake.Server.Services.SessionManager.Models;

namespace Datalake.Server.Services.SessionManager;

/// <summary>
/// Менеджер сессий пользователей
/// </summary>
/// <param name="loggerFactory"></param>
public class SessionManagerService(ILoggerFactory loggerFactory)
{
	private readonly ILogger<SessionManagerService> _logger = loggerFactory.CreateLogger<SessionManagerService>();

	/// <summary>
	/// Список текущих сессий
	/// </summary>
	List<AuthSession> Sessions { get; set; } = [];

	/// <summary>
	/// Список статичных учетных записей
	/// </summary>
	public static List<AuthSession> StaticAuthRecords { get; set; } = [];

	/// <summary>
	/// Получить текущую сессию по токену
	/// </summary>
	/// <param name="token">Токен сессии</param>
	/// <param name="address">Адрес, с которого разрешен доступ по статичной учетной записи</param>
	/// <returns>Информация о сессии</returns>
	public AuthSession? GetExistSession(string token, string address)
	{
		_logger.LogDebug("Search session from {address} with token [{token}]", address, token);
		foreach (var record in StaticAuthRecords)
		{
			_logger.LogDebug("Exists static user: {name} for {address} with token [{token}]",
				record.User.FullName, string.IsNullOrEmpty(record.StaticHost) ? record.StaticHost : "everywhere", record.User.Token);
		}

		var session = Sessions.FirstOrDefault(x => x.User.Token == token)
			?? StaticAuthRecords
				.Where(x => x.User.Token == token)
				.Where(x => string.IsNullOrEmpty(x.StaticHost) || x.StaticHost == address)
				.FirstOrDefault();
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
		var address = context.Connection.RemoteIpAddress;
		if (!string.IsNullOrEmpty(token))
		{
			var tokenValue = token.ToString();
			var session = GetExistSession(tokenValue, address?.ToString() ?? string.Empty);
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
	}

	/// <summary>
	/// Создание новой сессии для пользователя
	/// </summary>
	/// <param name="userAuthInfo">Информация о пользователе</param>
	/// <param name="isStatic">Является ли пользователь статичным</param>
	/// <returns>Информация о сессии</returns>
	public AuthSession OpenSession(UserAuthInfo userAuthInfo, bool isStatic = false)
	{
		Sessions.RemoveAll(x => x.User.Guid == userAuthInfo.Guid);

		var session = new AuthSession
		{
			User = userAuthInfo,
			ExpirationTime = isStatic ? DateTime.MaxValue : DateTime.UtcNow.AddDays(7), // срок жизни сессии
			StaticHost = string.Empty,
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
		var session = GetExistSession(token, string.Empty);
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
