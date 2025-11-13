using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Models.Users;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Users.Queries.GetUserWithDetails;

public interface IGetUserWithDetailsHandler : IQueryHandler<GetUserWithDetailsQuery, UserInfo> { }

public class GetUserWithDetailsHandler(
	IUsersQueriesService usersQueriesService) : IGetUserWithDetailsHandler
{
	public async Task<UserInfo> HandleAsync(GetUserWithDetailsQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoGlobalAccess(AccessType.Manager);

		var data = await usersQueriesService.GetByGuidAsync(query.Guid, ct)
			?? throw InventoryNotFoundException.NotFoundUser(query.Guid);

		return data;
	}
}
