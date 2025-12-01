using Datalake.Domain.Enums;
using Datalake.Gateway.Application.Interfaces;
using Datalake.Gateway.Application.Interfaces.Storage;
using Datalake.Shared.Application.Interfaces;
using Datalake.Shared.Application.Interfaces.AccessRules;

namespace Datalake.Gateway.Application.Features.Users.Queries.GetUsersActivity;

public interface IGetUsersActivityHandler : IQueryHandler<GetUsersActivityQuery, Dictionary<Guid, DateTime?>> { }

public class GetUsersActivityHandler(
	ISessionsService sessionsService,
	IUsersAccessStore userAccessStore,
	IUsersActivityStore usersActivityService) : IGetUsersActivityHandler
{
	public async Task<Dictionary<Guid, DateTime?>> HandleAsync(GetUsersActivityQuery query, CancellationToken ct = default)
	{
		var sessionInfo = await sessionsService.GetAsync(query.Token, ct);
		var user = userAccessStore.Get(sessionInfo.UserGuid)
			?? throw new ApplicationException("Пользователь не найден");

		user.ThrowIfNoGlobalAccess(AccessType.Manager);

		Dictionary<Guid, DateTime?> activity = [];

		foreach (var guid in query.Users)
			activity[guid] = usersActivityService.Get(guid);

		return activity;
	}
}
