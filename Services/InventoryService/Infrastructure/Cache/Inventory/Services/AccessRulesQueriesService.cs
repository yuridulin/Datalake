using Datalake.InventoryService.Domain.Constants;
using Datalake.InventoryService.Domain.Queries;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.AccessRights;
using Datalake.PublicApi.Models.Blocks;
using Datalake.PublicApi.Models.Sources;
using Datalake.PublicApi.Models.Tags;
using Datalake.PublicApi.Models.UserGroups;
using Datalake.PublicApi.Models.Users;

namespace Datalake.InventoryService.Infrastructure.Cache.Inventory.Services;

public class AccessRulesQueriesService(IInventoryCache inventoryCache) : IAccessRulesQueriesService
{
	public Task<IEnumerable<AccessRightsInfo>> GetAsync(
		Guid? userGuid = null,
		Guid? userGroupGuid = null,
		int? sourceId = null,
		int? blockId = null,
		int? tagId = null)
	{
		var state = inventoryCache.State;

		var data = state.AccessRules
			.Where(rule => !rule.IsGlobal)
			.Where(rule => !userGuid.HasValue || rule.UserGuid == userGuid)
			.Where(rule => !userGroupGuid.HasValue || rule.UserGroupGuid == userGroupGuid)
			.Where(rule => !sourceId.HasValue || rule.SourceId == sourceId)
			.Where(rule => !blockId.HasValue || rule.BlockId == blockId)
			.Where(rule => !tagId.HasValue || rule.TagId == tagId)
			.Select(rule => new AccessRightsInfo
			{
				Id = rule.Id,
				AccessType = rule.AccessType,
				IsGlobal = rule.IsGlobal,
				User = !state.UsersByGuid.TryGetValue(rule.UserGuid ?? Guid.Empty, out var user) ? null : new UserSimpleInfo
				{
					Guid = user.Guid,
					FullName = user.FullName ?? string.Empty,
				},
				UserGroup = !state.UserGroupsByGuid.TryGetValue(rule.UserGroupGuid ?? Guid.Empty, out var usergroup) ? null : new UserGroupSimpleInfo
				{
					Guid = usergroup.Guid,
					Name = usergroup.Name,
				},
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
					SourceType = state.SourcesById.TryGetValue(rule.SourceId ?? Identifiers.UnsetSource, out var tagSource) ? tagSource.Type : SourceType.NotSet,
				},
			});

		return Task.FromResult(data);
	}
}
