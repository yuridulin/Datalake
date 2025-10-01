using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Queries;
using Datalake.PublicApi.Models.Users;

namespace Datalake.InventoryService.Application.Features.Users.Queries.GetUserWithDetails;

public interface IGetUserWithDetailsHandler : IQueryHandler<GetUserWithDetailsQuery, UserDetailInfo> { }

public class GetUserWithDetailsHandler(
	IUsersQueriesService usersQueriesService) : IGetUserWithDetailsHandler
{
	public async Task<UserDetailInfo> HandleAsync(GetUserWithDetailsQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoGlobalAccess(PublicApi.Enums.AccessType.Manager);

		var data = await usersQueriesService.GetWithDetailsAsync(query.Guid, ct)
			?? throw Errors.NotFoundUser(query.Guid);

		return data;
	}
}
