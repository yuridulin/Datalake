using Datalake.Contracts.Internal.Constants;
using Datalake.Gateway.Host.Interfaces;
using Datalake.Shared.Application.Exceptions;

namespace Datalake.Gateway.Host.Services;

public class SessionTokenExtractor : ISessionTokenExtractor
{
	public string ExtractToken(HttpRequest request)
	{
		var sessionToken = request.Headers[Headers.SessionTokenHeander].FirstOrDefault();
		return sessionToken ?? throw new UnauthenticatedException("Токен сессии не получен");
	}

	public string ExtractToken(HttpContext context)
	{
		return ExtractToken(context.Request);
	}
}
