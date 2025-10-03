using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Shared.Api.Constants;
using Datalake.Shared.Api.Interfaces;
using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Application.Entities;

namespace Datalake.Inventory.Host.Services;

[Singleton]
public class AuthenticationService(IUserAccessCache cache) : IAuthenticator
{
	public UserAccessEntity Authenticate(HttpContext httpContext)
	{
		// проверка внешнего (основного) пользователя
		if (!httpContext.Request.Headers.TryGetValue(Headers.UserHeader, out var userGuidString))
			throw new ArgumentException("Идентификатор пользователя не прочитан из заголовка");

		if (!Guid.TryParse(userGuidString, out var userGuid))
			throw new InvalidCastException("Идентификатор пользователя не прочитан как GUID");

		cache.State.TryGet(userGuid, out var user);
		if (user == null)
			throw new KeyNotFoundException($"Внешний пользователь не найден по идентификатору: {userGuid}");

		// проверка внутреннего пользователя, если его хотели передать
		if (httpContext.Request.Headers.ContainsKey(Headers.UnderlyingUserHeader))
		{
			if (!httpContext.Request.Headers.TryGetValue(Headers.UnderlyingUserHeader, out var underlyingUserGuidString))
				throw new ArgumentException("Идентификатор внутреннего пользователя не прочитан из заголовка");

			if (!Guid.TryParse(underlyingUserGuidString, out var underlyingUserGuid))
				throw new InvalidCastException("Идентификатор внутреннего пользователя не прочитан как GUID");

			cache.State.TryGet(underlyingUserGuid, out var underlyingUser);
			if (underlyingUser == null)
				throw new KeyNotFoundException($"Внутренний пользователь не найден по идентификатору: {userGuid}");

			user.AddUnderlyingUser(underlyingUser);
		}

		return user;
	}
}
