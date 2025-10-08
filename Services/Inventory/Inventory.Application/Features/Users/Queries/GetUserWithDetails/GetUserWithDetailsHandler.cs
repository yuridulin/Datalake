using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Api.Models.Users;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Users.Queries.GetUserWithDetails;

public interface IGetUserWithDetailsHandler : IQueryHandler<GetUserWithDetailsQuery, UserDetailInfo> { }

public class GetUserWithDetailsHandler(
	IUsersQueriesService usersQueriesService) : IGetUserWithDetailsHandler
{
	public async Task<UserDetailInfo> HandleAsync(GetUserWithDetailsQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoGlobalAccess(AccessType.Manager);

		var data = await usersQueriesService.GetWithDetailsAsync(query.Guid, ct)
			?? throw InventoryNotFoundException.NotFoundUser(query.Guid);

		return data;
	}
}
