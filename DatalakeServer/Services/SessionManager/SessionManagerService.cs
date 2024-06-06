using DatalakeApiClasses.Models.Users;
using DatalakeServer.Constants;
using DatalakeServer.Services.SessionManager.Models;

namespace DatalakeServer.Services.SessionManager;

public class SessionManagerService
{
	List<AuthSession> Sessions { get; set; } = [];

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

	public void AddSessionToResponse(AuthSession session, HttpResponse response)
	{
		response.Headers[AuthConstants.TokenHeader] = session.User.Token;
		response.Headers[AuthConstants.NameHeader] = session.User.UserName;
	}

	public AuthSession OpenSession(UserAuthInfo userAuthInfo)
	{
		Sessions.RemoveAll(x => x.User.UserName == userAuthInfo.UserName);

		var session = new AuthSession
		{
			User = userAuthInfo,
			ExpirationTime = DateTime.UtcNow.AddDays(7), // срок жизни сессии
		};
		session.User.Token = new Random().Next().ToString();
		Sessions.Add(session);

		return session;
	}

	public void CloseSession(string token)
	{
		var session = GetExistSession(token);
		if (session != null)
		{
			RemoveSession(session);
		}
	}

	void RemoveSession(AuthSession session)
	{
		Sessions.Remove(session);
	}
}
