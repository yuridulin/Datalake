using Datalake.InventoryService.Domain.Constants;
using Datalake.InventoryService.Domain.Queries;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.AccessRights;
using Datalake.PublicApi.Models.Blocks;
using Datalake.PublicApi.Models.Sources;
using Datalake.PublicApi.Models.Tags;
using Datalake.PublicApi.Models.UserGroups;

namespace Datalake.InventoryService.Infrastructure.Cache.Inventory.Services;

public class UsersGroupsQueriesService(IInventoryCache inventoryCache) : IUsersGroupsQueriesService
{
	public Task<IEnumerable<UserGroupInfo>> GetAsync()
	{
		var state = inventoryCache.State;

		var data = state.UserGroups
			.Where(userGroup => !userGroup.IsDeleted)
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

	public Task<IEnumerable<UserGroupDetailedInfo>> GetWithDetailsAsync()
	{
		var state = inventoryCache.State;
		var activeUserGroups = state.UserGroups.Where(userGroup => !userGroup.IsDeleted);

		var data = activeUserGroups
			.Join(
				state.AccessRules.Where(rule => rule.IsGlobal && rule.UserGroupGuid.HasValue),
				userGroup => userGroup.Guid,
				rule => rule.UserGroupGuid,
				(userGroup, rule) => new UserGroupDetailedInfo
				{
					Guid = userGroup.Guid,
					Name = userGroup.Name,
					Description = userGroup.Description,
					ParentGroupGuid = userGroup.ParentGuid,
					GlobalAccessType = rule.AccessType,
					Users = state.UserGroupRelations
						.Where(relation => relation.UserGroupGuid == userGroup.Guid)
						.Join(
							state.Users.Where(user => !user.IsDeleted),
							relation => relation.UserGuid,
							user => user.Guid,
							(relation, user) => new UserGroupUsersInfo
							{
								Guid = user.Guid,
								FullName = user.FullName,
								AccessType = relation.AccessType,
							})
						.ToArray(),
					AccessRights = state.AccessRules
						.Where(rule => !rule.IsGlobal && rule.UserGroupGuid == userGroup.Guid)
						.Select(rule => new AccessRightsForOneInfo
						{
							Id = rule.Id,
							IsGlobal = rule.IsGlobal,
							AccessType = rule.AccessType,
							Source = !state.SourcesById.TryGetValue(rule.SourceId ?? Identifiers.UnsetSource, out var source) ? null : new SourceSimpleInfo
							{
								Id = source.Id,
								Name = source.Name,
							},
							Block = !state.BlocksById.TryGetValue(rule.BlockId ?? 0, out var block) ? null : new BlockSimpleInfo
							{
								Id = block.Id,
								Guid = block.GlobalId,
								Name = block.Name,
							},
							Tag = !state.TagsById.TryGetValue(rule.TagId ?? 0, out var tag) ? null : new TagSimpleInfo
							{
								Id = tag.Id,
								Guid = tag.GlobalGuid,
								Name = tag.Name,
								Type = tag.Type,
								Resolution = tag.Resolution,
								SourceType = !state.SourcesById.TryGetValue(tag.SourceId, out var tagSource) ? SourceType.NotSet : tagSource.Type,
							},
						})
						.Where(x => x.Tag != null || x.Block != null || x.Source != null)
						.ToArray(),
					Subgroups = activeUserGroups
						.Where(subGroup => subGroup.ParentGuid == userGroup.Guid)
						.Select(subGroup => new UserGroupSimpleInfo
						{
							Guid = subGroup.Guid,
							Name = subGroup.Name,
						}
					).ToArray(),
				});

		return Task.FromResult(data);
	}
}
