using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Models.UserGroups;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroup;

public interface IGetUserGroupHandler : IQueryHandler<GetUserGroupQuery, UserGroupInfo> { }

public class GetUserGroupHandler(
	IUsersGroupsQueriesService usersGroupsQueriesService) : IGetUserGroupHandler
{
	public async Task<UserGroupInfo> HandleAsync(GetUserGroupQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoAccessToUserGroup(AccessType.Viewer, query.Guid);

		var data = await usersGroupsQueriesService.GetAsync(query.Guid, ct)
			?? throw InventoryNotFoundException.NotFoundUserGroup(query.Guid);

		return data;
	}
}
