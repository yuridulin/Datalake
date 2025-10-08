using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Api.Models.Users;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Users.Queries.GetUsers;

public interface IGetUsersHandler : IQueryHandler<GetUsersQuery, IEnumerable<UserInfo>> { }

public class GetUsersHandler(
	IUsersQueriesService usersQueriesService) : IGetUsersHandler
{
	public async Task<IEnumerable<UserInfo>> HandleAsync(GetUsersQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoGlobalAccess(AccessType.Manager);

		var data = await usersQueriesService.GetAsync(ct);

		return data;
	}
}
