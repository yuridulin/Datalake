using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Application.Interfaces.AccessRules;
using Datalake.Shared.Hosting.Constants;
using Datalake.Shared.Hosting.Interfaces;

namespace Datalake.Data.Host.Services;

[Singleton]
public class AuthenticationService(IUsersAccessStore userAccessStore) : IAuthenticator
{
	public UserAccessValue Authenticate(HttpContext httpContext)
	{
		// проверка внешнего (основного) пользователя
		if (!httpContext.Request.Headers.TryGetValue(Headers.UserGuidHeader, out var userGuidString))
			throw new ArgumentException("Идентификатор пользователя не прочитан из заголовка");

		if (!Guid.TryParse(userGuidString, out var userGuid))
			throw new InvalidCastException("Идентификатор пользователя не прочитан как GUID");

		var user = userAccessStore.Get(userGuid)
			?? throw new KeyNotFoundException($"Пользователь не найден по идентификатору: {userGuid}");

		return user;
	}
}
