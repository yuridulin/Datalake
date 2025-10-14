using Datalake.Data.Application.Interfaces.Cache;
using Datalake.Shared.Api.Constants;
using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Hosting.Interfaces;

namespace Datalake.Data.Host.Services;

[Singleton]
public class AuthenticationService(IUserAccessStore cache) : IAuthenticator
{
	public UserAccessValue Authenticate(HttpContext httpContext)
	{
		// проверка внешнего (основного) пользователя
		if (!httpContext.Request.Headers.TryGetValue(Headers.UserGuidHeader, out var userGuidString))
			throw new ArgumentException("Идентификатор пользователя не прочитан из заголовка");

		if (!Guid.TryParse(userGuidString, out var userGuid))
			throw new InvalidCastException("Идентификатор пользователя не прочитан как GUID");

		var user = cache.TryGet(userGuid)
			?? throw new KeyNotFoundException($"Пользователь не найден по идентификатору: {userGuid}");

		return user;
	}
}
