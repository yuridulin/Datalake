using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Queries;
using Datalake.PublicApi.Models.Users;

namespace Datalake.InventoryService.Application.Features.Users.Queries.GetUsers;

public interface IGetUsersHandler : IQueryHandler<GetUsersQuery, IEnumerable<UserInfo>> { }

public class GetUsersHandler(
	IUsersQueriesService usersQueriesService) : IGetUsersHandler
{
	public async Task<IEnumerable<UserInfo>> HandleAsync(GetUsersQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoGlobalAccess(PublicApi.Enums.AccessType.Manager);

		var data = await usersQueriesService.GetAsync(ct);

		return data;
	}
}
