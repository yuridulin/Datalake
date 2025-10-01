using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Queries;
using Datalake.PublicApi.Models.UserGroups;

namespace Datalake.InventoryService.Application.Features.UserGroups.Queries.GetUserGroup;

public interface IGetUserGroupHandler : IQueryHandler<GetUserGroupQuery, UserGroupInfo> { }

public class GetUserGroupHandler(
	IUsersGroupsQueriesService usersGroupsQueriesService) : IGetUserGroupHandler
{
	public async Task<UserGroupInfo> HandleAsync(GetUserGroupQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoAccessToUserGroup(PublicApi.Enums.AccessType.Viewer, query.Guid);

		var data = await usersGroupsQueriesService.GetAsync(query.Guid, ct)
			?? throw Errors.NotFoundUserGroup(query.Guid);

		return data;
	}
}
