using Datalake.Gateway.Host.Interfaces;
using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Application.Exceptions;
using Datalake.Shared.Hosting.Constants;

namespace Datalake.Gateway.Host.Services;

/// <inheritdoc cref="ISessionTokenExtractor" />
[Scoped]
public class SessionTokenExtractor : ISessionTokenExtractor
{
	/// <inheritdoc/>
	public string ExtractToken(HttpRequest request)
	{
		var sessionToken = request.Headers[Headers.SessionTokenHeander].FirstOrDefault();
		return sessionToken ?? throw new UnauthenticatedException("Токен сессии не получен");
	}

	/// <inheritdoc/>
	public string ExtractToken(HttpContext context)
	{
		return ExtractToken(context.Request);
	}
}
