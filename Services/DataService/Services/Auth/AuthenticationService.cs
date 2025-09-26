using Datalake.DataService.Abstractions;
using Datalake.PrivateApi.Attributes;
using Datalake.PrivateApi.Entities;

namespace Datalake.DataService.Services.Auth;

[Singleton]
public class AuthenticationService(IAccessStore store) : IAuthenticatorService
{
	const string UserHeader = "X-Forwarded-User";
	const string UnderlyingUserHeader = "X-Forwarded-Underlying-User";

	public UserAccessEntity Authenticate(HttpContext httpContext)
	{
		// проверка внешнего (основного) пользователя
		if (!httpContext.Request.Headers.TryGetValue(UserHeader, out var userGuidString))
			throw new ArgumentException("Идентификатор пользователя не прочитан из заголовка");

		if (!Guid.TryParse(userGuidString, out var userGuid))
			throw new InvalidCastException("Идентификатор пользователя не прочитан как GUID");

		var user = store.TryGet(userGuid)
			?? throw new KeyNotFoundException($"Внешний пользователь не найден по идентификатору: {userGuid}");

		// проверка внутреннего пользователя, если его хотели передать
		if (httpContext.Request.Headers.ContainsKey(UnderlyingUserHeader))
		{
			if (!httpContext.Request.Headers.TryGetValue(UnderlyingUserHeader, out var underlyingUserGuidString))
				throw new ArgumentException("Идентификатор внутреннего пользователя не прочитан из заголовка");

			if (!Guid.TryParse(underlyingUserGuidString, out var underlyingUserGuid))
				throw new InvalidCastException("Идентификатор внутреннего пользователя не прочитан как GUID");

			var underlyingUser = store.TryGet(underlyingUserGuid)
				?? throw new KeyNotFoundException($"Внутренний пользователь не найден по идентификатору: {userGuid}");

			user.AddUnderlyingUser(underlyingUser);
		}

		return user;
	}
}