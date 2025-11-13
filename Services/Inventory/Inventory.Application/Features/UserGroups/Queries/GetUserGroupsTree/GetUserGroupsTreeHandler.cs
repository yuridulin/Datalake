using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Models.UserGroups;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroupsTree;

public interface IGetUserGroupsTreeHandler : IQueryHandler<GetUserGroupsTreeQuery, IEnumerable<UserGroupTreeInfo>> { }

public class GetUserGroupsTreeHandler(
	IUsersGroupsQueriesService usersGroupsQueriesService) : IGetUserGroupsTreeHandler
{
	public async Task<IEnumerable<UserGroupTreeInfo>> HandleAsync(GetUserGroupsTreeQuery query, CancellationToken ct = default)
	{
		var data = await usersGroupsQueriesService.GetAsync(ct);

		var tree = GetChildren(query, data, null);

		return tree;
	}

	private static UserGroupTreeInfo[] GetChildren(GetUserGroupsTreeQuery query, IEnumerable<UserGroupInfo> groups, Guid? guid)
	{
		return groups
			.Where(x => x.ParentGroupGuid == guid)
			.Select(x =>
			{
				var rule = query.User.GetAccessToUserGroup(x.Guid);

				var group = new UserGroupTreeInfo
				{
					Guid = x.Guid,
					Name = x.Name,
					ParentGuid = x.ParentGroupGuid,
					Description = x.Description,
					AccessRule = new(rule.Id, rule.Access),
					GlobalAccessType = x.GlobalAccessType,
					ParentGroupGuid = x.ParentGroupGuid,
					Children = GetChildren(query, groups, x.Guid),
				};

				if (!rule.HasAccess(AccessType.Viewer))
				{
					group.Name = string.Empty;
					group.Description = string.Empty;
				}

				return group;
			})
			.Where(x => x.Children.Length > 0)
			.ToArray();
	}
}