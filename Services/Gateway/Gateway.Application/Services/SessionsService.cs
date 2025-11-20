using Datalake.Domain.Entities;
using Datalake.Domain.Exceptions;
using Datalake.Gateway.Application.Interfaces;
using Datalake.Gateway.Application.Interfaces.Repositories;
using Datalake.Gateway.Application.Interfaces.Storage;
using Datalake.Gateway.Application.Models;
using Datalake.Shared.Application.Attributes;

namespace Datalake.Gateway.Application.Services;

[Scoped]
public class SessionsService(
	IUsersActivityStore usersActivityStore,
	ISessionsStore cache,
	IUnitOfWork unitOfWork,
	IUserSessionsRepository repository) : ISessionsService
{
	public async Task<UserSessionInfo> GetAsync(string sessionToken, CancellationToken ct = default)
	{
		UserSession session = await cache.GetAsync(sessionToken)
			?? await repository.GetByTokenAsync(sessionToken, ct)
			?? throw new ApplicationException("Сессия не найдена по токену: " + sessionToken);

		// Проверяем валидность сессии
		if (session.IsExpire())
		{
			// Автоматически удаляем просроченную сессию
			await repository.DeleteAsync(session, ct);
			await unitOfWork.SaveChangesAsync(ct);
			await cache.RemoveAsync(sessionToken);
			throw new DomainException("Сессия истекла");
		}

		usersActivityStore.Set(session.UserGuid);

		var info = new UserSessionInfo
		{
			Token = session.Token.Value,
			UserGuid = session.UserGuid,
			Type = session.Type,
			ExpirationTime = session.ExpirationTime,
		};

		return info;
	}

	public async Task<string> OpenAsync(User user, CancellationToken ct = default)
	{
		UserSession session;

		var existSession = await repository.GetByGuidAsync(user.Guid, ct);
		if (existSession == null)
		{
			session = UserSession.Create(user.Guid, user.Type, null);
			await repository.AddAsync(session, ct);
		}
		else if (existSession.IsExpire())
		{
			await repository.DeleteAsync(existSession, ct);

			session = UserSession.Create(user.Guid, user.Type, null);
			await repository.AddAsync(session, ct);
		}
		else
		{
			existSession.Refresh(TimeSpan.FromDays(7));
			await repository.UpdateAsync(existSession, ct);

			session = existSession;
		}

		usersActivityStore.Set(session.UserGuid);
		await cache.SetAsync(session.Token.Value, session);

		return session.Token.Value;
	}

	public async Task CloseAsync(string token, CancellationToken ct = default)
	{
		var session = await repository.GetByTokenAsync(token, ct);

		if (session != null)
			await repository.DeleteAsync(session, ct);

		await cache.RemoveAsync(token);
	}
}
