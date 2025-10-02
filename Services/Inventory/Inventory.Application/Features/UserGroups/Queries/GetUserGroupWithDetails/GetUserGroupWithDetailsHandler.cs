using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Queries;
using Datalake.Inventory.Api.Models.UserGroups;

namespace Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroupWithDetails;

public interface IGetUserGroupWithDetailsHandler : IQueryHandler<GetUserGroupWithDetailsQuery, UserGroupDetailedInfo> { }

public class GetUserGroupWithDetailsHandler(
	IUsersGroupsQueriesService usersGroupsQueriesService) : IGetUserGroupWithDetailsHandler
{
	public async Task<UserGroupDetailedInfo> HandleAsync(GetUserGroupWithDetailsQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoAccessToUserGroup(AccessType.Viewer, query.Guid);

		var data = await usersGroupsQueriesService.GetWithDetailsAsync(query.Guid, ct)
			?? throw InventoryNotFoundException.NotFoundUserGroup(query.Guid);

		return data;
	}
}
