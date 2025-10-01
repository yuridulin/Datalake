using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Queries;
using Datalake.PublicApi.Models.UserGroups;

namespace Datalake.InventoryService.Application.Features.UserGroups.Queries.GetUserGroupWithDetails;

public interface IGetUserGroupWithDetailsHandler : IQueryHandler<GetUserGroupWithDetailsQuery, UserGroupDetailedInfo> { }

public class GetUserGroupWithDetailsHandler(
	IUsersGroupsQueriesService usersGroupsQueriesService) : IGetUserGroupWithDetailsHandler
{
	public async Task<UserGroupDetailedInfo> HandleAsync(GetUserGroupWithDetailsQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoAccessToUserGroup(PublicApi.Enums.AccessType.Viewer, query.Guid);

		var data = await usersGroupsQueriesService.GetWithDetailsAsync(query.Guid, ct)
			?? throw Errors.NotFoundUserGroup(query.Guid);

		return data;
	}
}
