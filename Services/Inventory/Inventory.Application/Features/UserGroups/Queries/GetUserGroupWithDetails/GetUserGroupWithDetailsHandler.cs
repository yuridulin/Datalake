using Datalake.Contracts.Models.AccessRules;
using Datalake.Contracts.Models.UserGroups;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroupWithDetails;

public interface IGetUserGroupWithDetailsHandler : IQueryHandler<GetUserGroupWithDetailsQuery, UserGroupDetailedInfo> { }

public class GetUserGroupWithDetailsHandler(
	IAccessRulesQueriesService accessRulesQueriesService,
	IUsersGroupsQueriesService usersGroupsQueriesService) : IGetUserGroupWithDetailsHandler
{
	public async Task<UserGroupDetailedInfo> HandleAsync(GetUserGroupWithDetailsQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoAccessToUserGroup(AccessType.Viewer, query.Guid);

		var userGroup = await usersGroupsQueriesService.GetAsync(query.Guid, ct)
			?? throw InventoryNotFoundException.NotFoundUserGroup(query.Guid);

		var userAccessRule = query.User.GetAccessToUserGroup(query.Guid);
		var rules = await accessRulesQueriesService.GetAsync(userGroupGuid: userGroup.Guid, ct: ct);

		return new UserGroupDetailedInfo
		{
			Guid = userGroup.Guid,
			Name = userGroup.Name,
			Description = userGroup.Description,
			ParentGroupGuid = userGroup.ParentGroupGuid,
			GlobalAccessType = userGroup.GlobalAccessType,
			AccessRule = new(userAccessRule.Id, userAccessRule.Access),
			Subgroups = await usersGroupsQueriesService.GetByParentGuidAsync(userGroup.Guid, ct),
			AccessRights = rules
				.Select(x => new AccessRightsForOneInfo
				{
					Id = x.Id,
					IsGlobal = x.IsGlobal,
					AccessType = x.AccessType,
					Block = x.Block,
					Source = x.Source,
					Tag = x.Tag,
				})
				.ToList(),
			Users = await usersGroupsQueriesService.GetMembersAsync(userGroup.Guid, ct),
		};
	}
}
