using Datalake.Domain.Enums;
using Datalake.Gateway.Application.Interfaces;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Gateway.Application.Features.Queries.GetUsersActivity;

public interface IGetUsersActivityHandler : IQueryHandler<GetUsersActivityQuery, IDictionary<Guid, DateTime?>> { }

public class GetUsersActivityHandler(
	ISessionsService sessionsService,
	IUserAccessStore userAccessService,
	IUsersActivityService usersActivityService) : IGetUsersActivityHandler
{
	public async Task<IDictionary<Guid, DateTime?>> HandleAsync(GetUsersActivityQuery query, CancellationToken ct = default)
	{
		var sessionInfo = await sessionsService.GetAsync(query.Token, ct);
		var user = userAccessService.TryGet(sessionInfo.UserGuid)
			?? throw new ApplicationException("Пользователь не найден");

		user.ThrowIfNoGlobalAccess(AccessType.Manager);

		Dictionary<Guid, DateTime?> activity = [];

		foreach (var guid in query.Users)
			activity[guid] = usersActivityService.Get(guid);

		return activity;
	}
}
