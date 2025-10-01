using Datalake.InventoryService.Application.Queries;
using Datalake.InventoryService.Infrastructure.Cache.Inventory.Constants;
using Datalake.PrivateApi.Exceptions;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.AccessRights;
using Datalake.PublicApi.Models.Blocks;
using Datalake.PublicApi.Models.Sources;
using Datalake.PublicApi.Models.Tags;
using Datalake.PublicApi.Models.UserGroups;

namespace Datalake.InventoryService.Infrastructure.Cache.Inventory.Services;

public class UsersGroupsQueriesService(IInventoryCache inventoryCache) : IUsersGroupsQueriesService
{
	public Task<UserGroupInfo?> GetAsync(
		Guid userGroupGuid,
		CancellationToken ct = default)
	{
		var state = inventoryCache.State;

		if (!state.ActiveUserGroupsByGuid.TryGetValue(userGroupGuid, out var userGroup))
			return Task.FromResult<UserGroupInfo?>(null);

		var rule = state.AccessRules
			.Where(rule => rule.IsGlobal && rule.UserGroupGuid == userGroupGuid)
			.FirstOrDefault()
			?? throw new InfrastructureException("У группы пользователей нет глобального правила доступа");

		var result = new UserGroupInfo
		{
			Guid = userGroup.Guid,
			Name = userGroup.Name,
			Description = userGroup.Description,
			ParentGroupGuid = userGroup.ParentGuid,
			GlobalAccessType = rule.AccessType,
		};

		return Task.FromResult<UserGroupInfo?>(result);
	}

	public Task<IEnumerable<UserGroupInfo>> GetAsync(
		CancellationToken ct = default)
	{
		var state = inventoryCache.State;

		var data = state.ActiveUserGroups
			.Join(
				state.AccessRules.Where(rule => rule.IsGlobal && rule.UserGroupGuid.HasValue),
				userGroup => userGroup.Guid,
				rule => rule.UserGroupGuid,
				(userGroup, rule) => new UserGroupInfo
				{
					Guid = userGroup.Guid,
					Name = userGroup.Name,
					Description = userGroup.Description,
					ParentGroupGuid = userGroup.ParentGuid,
					GlobalAccessType = rule.AccessType,
				});

		return Task.FromResult(data);
	}

	public Task<UserGroupDetailedInfo?> GetWithDetailsAsync(
		Guid userGroupGuid,
		CancellationToken ct = default)
	{
		var state = inventoryCache.State;

		if (!state.ActiveUserGroupsByGuid.TryGetValue(userGroupGuid, out var userGroup))
			return Task.FromResult<UserGroupDetailedInfo?>(null);

		var rule = state.AccessRules
			.Where(rule => rule.IsGlobal && rule.UserGroupGuid == userGroupGuid)
			.FirstOrDefault()
			?? throw new InfrastructureException("У группы пользователей нет глобального правила доступа");

		var groupUsers = state.UserGroupRelations
			.Where(relation => relation.UserGroupGuid == userGroup.Guid)
			.Join(
				state.ActiveUsers,
				relation => relation.UserGuid,
				user => user.Guid,
				(relation, user) => new UserGroupUsersInfo
				{
					Guid = user.Guid,
					FullName = user.FullName,
					AccessType = relation.AccessType,
				})
			.ToArray();

		var groupRules = state.AccessRules
			.Where(rule => !rule.IsGlobal && rule.UserGroupGuid == userGroup.Guid)
			.Select(rule => new AccessRightsForOneInfo
			{
				Id = rule.Id,
				IsGlobal = rule.IsGlobal,
				AccessType = rule.AccessType,
				Source = !state.ActiveSourcesById.TryGetValue(rule.SourceId ?? Identifiers.UnsetSource, out var source) ? null : new SourceSimpleInfo
				{
					Id = source.Id,
					Name = source.Name,
				},
				Block = !state.ActiveBlocksById.TryGetValue(rule.BlockId ?? 0, out var block) ? null : new BlockSimpleInfo
				{
					Id = block.Id,
					Guid = block.GlobalId,
					Name = block.Name,
				},
				Tag = !state.ActiveTagsById.TryGetValue(rule.TagId ?? 0, out var tag) ? null : new TagSimpleInfo
				{
					Id = tag.Id,
					Guid = tag.GlobalGuid,
					Name = tag.Name,
					Type = tag.Type,
					Resolution = tag.Resolution,
					SourceType = !state.ActiveSourcesById.TryGetValue(tag.SourceId, out var tagSource) ? SourceType.NotSet : tagSource.Type,
				},
			})
			.Where(x => x.Tag != null || x.Block != null || x.Source != null)
			.ToArray();

		var groupChildren = state.ActiveUserGroups
			.Where(subGroup => subGroup.ParentGuid == userGroup.Guid)
			.Select(subGroup => new UserGroupSimpleInfo
			{
				Guid = subGroup.Guid,
				Name = subGroup.Name,
			})
			.ToArray();

		var result = new UserGroupDetailedInfo
		{
			Guid = userGroup.Guid,
			Name = userGroup.Name,
			Description = userGroup.Description,
			ParentGroupGuid = userGroup.ParentGuid,
			GlobalAccessType = rule.AccessType,
			Users = groupUsers,
			AccessRights = groupRules,
			Subgroups = groupChildren,
		};

		return Task.FromResult<UserGroupDetailedInfo?>(result);
	}
}
