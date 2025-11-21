using Datalake.Domain.ValueObjects;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Hosting.Constants;
using Datalake.Shared.Hosting.Interfaces;

namespace Datalake.Inventory.Host.Services;

[Singleton]
public class AuthenticationService(IUsersAccessStore cache) : IAuthenticator
{
	public UserAccessValue Authenticate(HttpContext httpContext)
	{
		if (!httpContext.Request.Headers.TryGetValue(Headers.UserGuidHeader, out var userGuidString))
			throw new ArgumentException("Идентификатор пользователя не прочитан из заголовка");

		if (!Guid.TryParse(userGuidString, out var userGuid))
			throw new InvalidCastException("Идентификатор пользователя не прочитан как GUID");

		if (cache.State.Version == 0)
			throw new ApplicationException("Система не готова к работе");

		if (!cache.State.UsersAccess.TryGetValue(userGuid, out var user))
			throw new KeyNotFoundException($"Пользователь не найден по идентификатору: {userGuid}");

		return user;
	}
}
