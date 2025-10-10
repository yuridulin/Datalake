using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Api.Models.AccessRules;
using Datalake.Inventory.Api.Models.Blocks;
using Datalake.Inventory.Api.Models.UserGroups;
using Datalake.Inventory.Api.Models.Users;
using Datalake.Inventory.Application.Queries;
using Microsoft.EntityFrameworkCore;
using static Datalake.Inventory.Api.Models.Blocks.BlockFullInfo;

namespace Datalake.Inventory.Infrastructure.Database.Queries;

public class BlocksQueriesService(InventoryDbContext context) : IBlocksQueriesService
{
	private IQueryable<BlockWithTagsInfo> QueryBlockWithTagInfo()
	{
		return context.Blocks
			.Where(x => !x.IsDeleted)
			.AsNoTracking()
			.Select(block => new BlockWithTagsInfo
			{
				Id = block.Id,
				Guid = block.GlobalId,
				Name = block.Name,
				Description = block.Description,
				ParentId = block.ParentId,
				Tags = block.RelationsToTags
					.Where(rel => rel.Tag != null && !rel.Tag.IsDeleted)
					.Select(rel => new BlockNestedTagInfo
					{
						Id = rel.Tag!.Id,
						Name = rel.Tag.Name,
						Guid = rel.Tag.GlobalGuid,
						RelationType = rel.Relation,
						LocalName = rel.Name ?? rel.Tag.Name,
						Type = rel.Tag.Type,
						Resolution = rel.Tag.Resolution,
						SourceId = rel.Tag.SourceId,
						SourceType = rel.Tag.Source == null || rel.Tag.Source.IsDeleted ? SourceType.Unset : rel.Tag.Source.Type,
					})
					.ToArray(),
			});
	}

	public async Task<IEnumerable<BlockWithTagsInfo>> GetWithTagsAsync(CancellationToken ct = default)
	{
		return await QueryBlockWithTagInfo().ToArrayAsync(ct);
	}

	public async Task<BlockFullInfo?> GetFullAsync(int blockId, CancellationToken ct = default)
	{
		var block = await QueryBlockWithTagInfo().FirstOrDefaultAsync(x => x.Id == blockId, ct);

		if (block == null)
			return null;

		var adults = new List<BlockTreeInfo>();

		// TODO: конченная штука, нужно переделать или на RecursiveCTE, или на использование вьюхи с Anchestors
		BlockParentInfo? firstParent = null;
		int? currentParentId = block.ParentId;
		while (currentParentId.HasValue)
		{
			var adult = await context.Blocks
				.Where(x => !x.IsDeleted && x.Id == currentParentId)
				.AsNoTracking()
				.Select(x => new BlockTreeInfo
				{
					Id = x.Id,
					Guid = x.GlobalId,
					Name = x.Name,
					ParentId = x.ParentId,
				})
				.FirstOrDefaultAsync(ct);

			if (adult != null)
			{
				adults.Add(adult);
				currentParentId = adult.ParentId;

				firstParent ??= new BlockParentInfo { Id = adult.Id, Name = adult.Name };
			}
		}

		var children = await context.Blocks
			.Where(x => x.ParentId == block.Id && !x.IsDeleted)
			.AsNoTracking()
			.Select(child => new BlockChildInfo
			{
				Id = child.Id,
				Name = child.Name,
			})
			.ToArrayAsync(ct);

		var properties = await context.BlockProperties
			.Where(x => x.BlockId == block.Id)
			.AsNoTracking()
			.Select(property => new BlockPropertyInfo
			{
				Id = property.Id,
				Name = property.Name,
				Type = property.Type,
				Value = property.Value,
			})
			.ToArrayAsync(ct);

		var rules = await context.AccessRights
				.Where(rule => rule.BlockId == block.Id)
				.AsNoTracking()
				.Select(rule => new AccessRightsForObjectInfo
				{
					Id = rule.Id,
					IsGlobal = rule.IsGlobal,
					AccessType = rule.AccessType,
					User = rule.User == null || rule.User.IsDeleted ? null : new UserSimpleInfo
					{
						Guid = rule.User.Guid,
						FullName = rule.User.FullName ?? string.Empty,
					},
					UserGroup = rule.UserGroup == null || rule.UserGroup.IsDeleted ? null : new UserGroupSimpleInfo
					{
						Guid = rule.UserGroup.Guid,
						Name = rule.UserGroup.Name,
					},
				})
				.ToArrayAsync(ct);

		var data = new BlockFullInfo
		{
			Id = block.Id,
			Guid = block.Guid,
			Name = block.Name,
			Description = block.Description,
			ParentId = block.ParentId,
			Tags = block.Tags,
			AccessRule = block.AccessRule,
			Parent = firstParent,
			Adults = adults.ToArray(),
			Children = children,
			Properties = properties,
			AccessRights = rules,
		};

		return data;
	}
}
