using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Shared.Api.Constants;
using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Hosting.Interfaces;

namespace Datalake.Inventory.Host.Services;

[Singleton]
public class AuthenticationService(IUserAccessCache cache) : IAuthenticator
{
	public UserAccessEntity Authenticate(HttpContext httpContext)
	{
		// проверка внешнего (основного) пользователя
		if (!httpContext.Request.Headers.TryGetValue(Headers.UserGuidHeader, out var userGuidString))
			throw new ArgumentException("Идентификатор пользователя не прочитан из заголовка");

		if (!Guid.TryParse(userGuidString, out var userGuid))
			throw new InvalidCastException("Идентификатор пользователя не прочитан как GUID");

		cache.State.TryGet(userGuid, out var user);
		if (user == null)
			throw new KeyNotFoundException($"Внешний пользователь не найден по идентификатору: {userGuid}");

		return user;
	}
}
