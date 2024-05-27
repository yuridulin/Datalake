using DatalakeApiClasses.Models.Users;
using DatalakeApp.Constants;
using DatalakeApp.Services.SessionManager.Models;

namespace DatalakeApp.Services.SessionManager;

public class SessionManagerService
{
	List<AuthSession> Sessions { get; set; } = [];

	public AuthSession? GetExistSession(string token)
	{
		var session = Sessions.FirstOrDefault(x => x.Token == token);
		if (session == null) return null;
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
			if (session == null) return null;
			AddSessionToResponse(session, context.Response);
			return session;
		}
		return null;
	}

	public void AddSessionToResponse(AuthSession session, HttpResponse response)
	{
		response.Headers[AuthConstants.TokenHeader] = session.Token;
		response.Headers[AuthConstants.AccessHeader] = session.AccessType.ToString();
		response.Headers[AuthConstants.NameHeader] = session.Login;
	}

	public AuthSession OpenSession(UserAuthInfo userAuthInfo)
	{
		Sessions.RemoveAll(x => x.Login == userAuthInfo.UserName);

		var session = new AuthSession
		{
			Token = new Random().Next().ToString(),
			ExpirationTime = DateTime.UtcNow.AddDays(7), // срок жизни сессии
			AccessType = userAuthInfo.AccessType,
			Login = userAuthInfo.UserName,
		};
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
