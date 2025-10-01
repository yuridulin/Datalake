using Datalake.InventoryService.Application.Queries;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.UserGroups;
using Datalake.PublicApi.Models.Users;
using System.Data;

namespace Datalake.InventoryService.Infrastructure.Cache.Inventory.Services;

public class UsersQueriesService(IInventoryCache inventoryCache) : IUsersQueriesService
{
	public Task<IEnumerable<UserInfo>> GetAsync()
	{
		var state = inventoryCache.State;
		var globalAccessRules = state.AccessRules.Where(rule => rule.IsGlobal);

		var userGroupsWithRules = state.UserGroups
			.Where(userGroup => !userGroup.IsDeleted)
			.Join(globalAccessRules, userGroup => userGroup.Guid, rule => rule.UserGroupGuid, (userGroup, groupRule) => new { userGroup, groupRule });

		var data = UsersDetailInfoFromState(state)
			.Select(user => new UserInfo
			{
				Login = user.Login,
				Guid = user.Guid,
				Type = user.Type,
				FullName = user.FullName,
				EnergoIdGuid = user.EnergoIdGuid,
				UserGroups = user.UserGroups,
				AccessType = user.AccessType,
			});

		return Task.FromResult(data);
	}

	public Task<IEnumerable<UserDetailInfo>> GetWithDetailsAsync()
	{
		var state = inventoryCache.State;
		var data = UsersDetailInfoFromState(state);

		return Task.FromResult(data);
	}

	private static IEnumerable<UserDetailInfo> UsersDetailInfoFromState(InventoryState state)
	{
		var globalAccessRules = state.AccessRules.Where(rule => rule.IsGlobal);

		var userGroupsWithRules = state.UserGroups
			.Where(userGroup => !userGroup.IsDeleted)
			.Join(globalAccessRules, userGroup => userGroup.Guid, rule => rule.UserGroupGuid, (userGroup, groupRule) => new { userGroup, groupRule });

		return state.Users
			.Where(user => !user.IsDeleted)
			.Join(
				globalAccessRules,
				user => user.Guid,
				rule => rule.UserGuid,
				(user, globalRule) =>
				{
					var userGroups = state.UserGroupRelations
						.Where(relation => relation.UserGuid == user.Guid)
						.Join(
							userGroupsWithRules,
							relation => relation.UserGroupGuid,
							userGroupBadge => userGroupBadge.userGroup.Guid,
							(_, userGroupBadge) => userGroupBadge)
						.ToArray();

					return new UserDetailInfo
					{
						Login = user.Login,
						Guid = user.Guid,
						Type = user.Type,
						FullName = user.FullName ?? string.Empty,
						EnergoIdGuid = user.EnergoIdGuid,
						UserGroups = userGroups
							.Select(badge => new UserGroupSimpleInfo
							{
								Guid = badge.userGroup.Guid,
								Name = badge.userGroup.Name,
							})
							.ToArray(),
						AccessType = userGroups
							.Select(badge => badge.groupRule.AccessType)
							.Append(globalRule.AccessType)
							.DefaultIfEmpty(AccessType.NoAccess)
							.Max(),
						Hash = user.PasswordHash,
						StaticHost = user.StaticHost,
					};
				});
	}
}
