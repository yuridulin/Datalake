using Datalake.Gateway.Application.Interfaces;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Gateway.Application.Features.Queries.GetUsersActivity;

public interface IGetUsersActivityHandler : IQueryHandler<GetUsersActivityQuery, IDictionary<Guid, DateTime?>> { }

public class GetUsersActivityHandler(
	IUsersActivityService usersActivityService) : IGetUsersActivityHandler
{
	public Task<IDictionary<Guid, DateTime?>> HandleAsync(GetUsersActivityQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoGlobalAccess(Contracts.Public.Enums.AccessType.Manager);

		Dictionary<Guid, DateTime?> activity = [];

		foreach (var guid in query.Users)
			activity[guid] = usersActivityService.Get(guid);

		return Task.FromResult<IDictionary<Guid, DateTime?>>(activity);
	}
}
