using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Queries;
using Datalake.Inventory.Infrastructure.Cache.Inventory.Constants;
using Datalake.Inventory.Api.Models.AccessRules;
using Datalake.Inventory.Api.Models.Blocks;
using Datalake.Inventory.Api.Models.Sources;
using Datalake.Inventory.Api.Models.Tags;
using Datalake.Inventory.Api.Models.UserGroups;
using Datalake.Inventory.Api.Models.Users;

namespace Datalake.Inventory.Infrastructure.Cache.Inventory.Queries;

public class AccessRulesQueriesService(IInventoryCache inventoryCache) : IAccessRulesQueriesService
{
	public Task<IEnumerable<AccessRightsInfo>> GetAsync(
		Guid? userGuid = null,
		Guid? userGroupGuid = null,
		int? sourceId = null,
		int? blockId = null,
		int? tagId = null,
		CancellationToken ct = default)
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
				User = !state.ActiveUsersByGuid.TryGetValue(rule.UserGuid ?? Guid.Empty, out var user) ? null : new UserSimpleInfo
				{
					Guid = user.Guid,
					FullName = user.FullName ?? string.Empty,
				},
				UserGroup = !state.ActiveUserGroupsByGuid.TryGetValue(rule.UserGroupGuid ?? Guid.Empty, out var usergroup) ? null : new UserGroupSimpleInfo
				{
					Guid = usergroup.Guid,
					Name = usergroup.Name,
				},
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
					SourceType = state.ActiveSourcesById.TryGetValue(rule.SourceId ?? Identifiers.UnsetSource, out var tagSource) ? tagSource.Type : SourceType.Unset,
				},
			});

		return Task.FromResult(data);
	}
}
