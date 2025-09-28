using Datalake.InventoryService.Domain.Queries;
using Datalake.PrivateApi.ValueObjects;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.AccessRights;
using Datalake.PublicApi.Models.Blocks;
using Datalake.PublicApi.Models.UserGroups;
using Datalake.PublicApi.Models.Users;
using static Datalake.PublicApi.Models.Blocks.BlockFullInfo;

namespace Datalake.InventoryService.Infrastructure.Cache.Inventory.Services;

public class BlocksQueriesService(IInventoryCache inventoryCache) : IBlocksQueriesService
{
	public Task<IEnumerable<BlockWithTagsInfo>> GetWithTagsAsync()
	{
		var state = inventoryCache.State;

		var data = state.Blocks
			.Where(block => !block.IsDeleted)
			.Select(block => new BlockWithTagsInfo
			{
				Id = block.Id,
				Guid = block.GlobalId,
				Name = block.Name,
				Description = block.Description,
				ParentId = block.ParentId,
				Tags = state.BlockTags
					.Where(relation => relation.BlockId == block.Id)
					.Join(state.Tags, relation => relation.TagId, tag => tag.Id, (relation, tag) => new BlockNestedTagInfo
					{
						Id = tag.Id,
						Name = tag.Name,
						Guid = tag.GlobalGuid,
						RelationType = relation.Relation,
						LocalName = relation.Name ?? tag.Name,
						Type = tag.Type,
						Resolution = tag.Resolution,
						SourceId = tag.SourceId,
						SourceType = state.SourcesById.TryGetValue(tag.SourceId, out var source) ? source.Type : SourceType.NotSet,
					})
					.ToArray(),
			});

		return Task.FromResult(data);
	}

	public Task<BlockFullInfo?> GetFullAsync(int blockId)
	{
		var state = inventoryCache.State;
		if (!state.BlocksById.TryGetValue(blockId, out var block))
			return Task.FromResult<BlockFullInfo?>(null);

		var adults = new List<BlockTreeInfo>();

		int? currentParentId = block.ParentId;
		do
		{
			if (state.BlocksById.TryGetValue(currentParentId ?? 0, out var adult))
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
			Parent = !state.BlocksById.TryGetValue(block.ParentId ?? 0, out var parent) ? null : new BlockParentInfo
			{
				Id = parent.Id,
				Name = parent.Name,
			},
			Adults = adults
				.ToArray(),
			Children = state.Blocks
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
				.Select(relation => !state.TagsById.TryGetValue(relation.TagId ?? 0, out var tag) ? null : new BlockNestedTagInfo
				{
					Id = tag.Id,
					Name = tag.Name,
					Guid = tag.GlobalGuid,
					RelationType = relation.Relation,
					LocalName = relation.Name ?? tag.Name,
					Type = tag.Type,
					Resolution = tag.Resolution,
					SourceId = tag.SourceId,
					SourceType = state.SourcesById.TryGetValue(tag.SourceId, out var source) ? source.Type : SourceType.NotSet,
				})
				.Where(x => x != null)
				.ToArray()!,
			AccessRights = state.AccessRights
				.Where(rule => rule.BlockId == block.Id)
				.Select(rule => new AccessRightsForObjectInfo
				{
					Id = rule.Id,
					IsGlobal = rule.IsGlobal,
					AccessType = rule.AccessType,
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
				})
				.ToArray(),
		};

		return Task.FromResult<BlockFullInfo?>(data);
	}

	public async Task<IEnumerable<BlockTreeInfo>> GetTreeAsync()
	{
		var data = await GetWithTagsAsync();
		var tree = GetChildren(data, null, string.Empty);
		return tree;
	}


	private static BlockTreeInfo[] GetChildren(IEnumerable<BlockWithTagsInfo> blocks, int? parentId, string prefix)
	{
		return blocks
			.Where(x => x.ParentId == parentId)
			.Select(x => new
			{
				Node = x,
				Children = GetChildren(blocks, x.Id, AppendPrefix(prefix, x.Name))
			})
			.Select(p =>
			{
				var rule = new AccessRuleValue(p.Node.AccessRule.RuleId, p.Node.AccessRule.Access);
				var hasViewer = rule.HasAccess(AccessType.Viewer);

				if (!hasViewer)
					return null!;

				if (p.Children.Length == 0)
					return null!;

				return new BlockTreeInfo
				{
					Id = p.Node.Id,
					Guid = p.Node.Guid,
					ParentId = p.Node.ParentId,
					Name = hasViewer ? p.Node.Name : string.Empty,
					FullName = AppendPrefix(prefix, p.Node.Name),
					Description = hasViewer ? p.Node.Description : string.Empty,
					Tags = hasViewer ? p.Node.Tags : Array.Empty<BlockNestedTagInfo>(),
					AccessRule = p.Node.AccessRule,
					Children = p.Children
				};
			})
			.Where(x => x != null)
			.OrderBy(x => x.Name)
			.ToArray();
	}

	private static string AppendPrefix(string prefix, string name) =>
		string.IsNullOrEmpty(prefix) ? name : $"{prefix}.{name}";
}
