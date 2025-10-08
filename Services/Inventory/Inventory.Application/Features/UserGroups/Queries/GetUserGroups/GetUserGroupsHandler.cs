using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Api.Models.UserGroups;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroups;

public interface IGetUserGroupsHandler : IQueryHandler<GetUserGroupsQuery, IEnumerable<UserGroupInfo>> { }

public class GetUserGroupsHandler(
	IUsersGroupsQueriesService usersGroupsQueriesService) : IGetUserGroupsHandler
{
	public async Task<IEnumerable<UserGroupInfo>> HandleAsync(GetUserGroupsQuery query, CancellationToken ct = default)
	{
		var data = await usersGroupsQueriesService.GetAsync(ct);

		foreach (var userGroupInfo in data)
		{
			var rule = query.User.GetAccessToUserGroup(userGroupInfo.Guid);

			userGroupInfo.AccessRule = new(rule.Id, rule.Access);

			if (!rule.HasAccess(AccessType.Viewer))
			{
				userGroupInfo.Name = string.Empty;
				userGroupInfo.Description = string.Empty;
			}
		}

		return data;
	}
}