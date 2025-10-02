using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Queries;
using Datalake.Inventory.Api.Models.AccessRules;
using Datalake.Inventory.Api.Models.Blocks;
using Datalake.Inventory.Api.Models.UserGroups;
using Datalake.Inventory.Api.Models.Users;
using static Datalake.Inventory.Api.Models.Blocks.BlockFullInfo;

namespace Datalake.Inventory.Infrastructure.Cache.Inventory.Queries;

public class BlocksQueriesService(IInventoryCache inventoryCache) : IBlocksQueriesService
{
	public Task<IEnumerable<BlockWithTagsInfo>> GetWithTagsAsync(CancellationToken ct = default)
	{
		var state = inventoryCache.State;

		var data = state.ActiveBlocks
			.Select(block => new BlockWithTagsInfo
			{
				Id = block.Id,
				Guid = block.GlobalId,
				Name = block.Name,
				Description = block.Description,
				ParentId = block.ParentId,
				Tags = state.BlockTags
					.Where(relation => relation.BlockId == block.Id)
					.Join(state.ActiveTags, relation => relation.TagId, tag => tag.Id, (relation, tag) => new BlockNestedTagInfo
					{
						Id = tag.Id,
						Name = tag.Name,
						Guid = tag.GlobalGuid,
						RelationType = relation.Relation,
						LocalName = relation.Name ?? tag.Name,
						Type = tag.Type,
						Resolution = tag.Resolution,
						SourceId = tag.SourceId,
						SourceType = state.ActiveSourcesById.TryGetValue(tag.SourceId, out var source) ? source.Type : SourceType.NotSet,
					})
					.ToArray(),
			});

		return Task.FromResult(data);
	}

	public Task<BlockFullInfo?> GetFullAsync(int blockId, CancellationToken ct = default)
	{
		var state = inventoryCache.State;
		if (!state.ActiveBlocksById.TryGetValue(blockId, out var block))
			return Task.FromResult<BlockFullInfo?>(null);

		var adults = new List<BlockTreeInfo>();

		int? currentParentId = block.ParentId;
		do
		{
			if (state.ActiveBlocksById.TryGetValue(currentParentId ?? 0, out var adult))
			{
				adults.Add(new BlockTreeInfo
				{
					Id = adult.Id,
					Guid = adult.GlobalId,
					Name = adult.Name,
					ParentId = adult.ParentId,
				});
				currentParentId = adult.ParentId;
			}
		}
		while ((currentParentId ?? 0) != 0);

		var data = new BlockFullInfo
		{
			Id = block.Id,
			Guid = block.GlobalId,
			Name = block.Name,
			Description = block.Description,
			ParentId = block.ParentId,
			Parent = !state.ActiveBlocksById.TryGetValue(block.ParentId ?? 0, out var parent) ? null : new BlockParentInfo
			{
				Id = parent.Id,
				Name = parent.Name,
			},
			Adults = adults
				.ToArray(),
			Children = state.ActiveBlocks
				.Where(child => child.ParentId == block.Id)
				.Select(child => new BlockChildInfo
				{
					Id = child.Id,
					Name = child.Name,
				})
				.ToArray(),
			Properties = state.BlockProperties
				.Where(property => property.BlockId == block.Id)
				.Select(property => new BlockPropertyInfo
				{
					Id = property.Id,
					Name = property.Name,
					Type = property.Type,
					Value = property.Value,
				})
				.ToArray(),
			Tags = state.BlockTags
				.Where(relation => relation.BlockId == block.Id)
				.Select(relation => !state.ActiveTagsById.TryGetValue(relation.TagId ?? 0, out var tag) ? null : new BlockNestedTagInfo
				{
					Id = tag.Id,
					Name = tag.Name,
					Guid = tag.GlobalGuid,
					RelationType = relation.Relation,
					LocalName = relation.Name ?? tag.Name,
					Type = tag.Type,
					Resolution = tag.Resolution,
					SourceId = tag.SourceId,
					SourceType = state.ActiveSourcesById.TryGetValue(tag.SourceId, out var source) ? source.Type : SourceType.NotSet,
				})
				.Where(x => x != null)
				.ToArray()!,
			AccessRights = state.AccessRules
				.Where(rule => rule.BlockId == block.Id)
				.Select(rule => new AccessRightsForObjectInfo
				{
					Id = rule.Id,
					IsGlobal = rule.IsGlobal,
					AccessType = rule.AccessType,
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
				})
				.ToArray(),
		};

		return Task.FromResult<BlockFullInfo?>(data);
	}
}
