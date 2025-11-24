using Datalake.Contracts.Models.Blocks;
using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Queries;
using Datalake.Inventory.Infrastructure.Database.Extensions;
using LinqToDB;

namespace Datalake.Inventory.Infrastructure.Database.Queries;

public class BlocksQueriesService(InventoryDbLinqContext context) : IBlocksQueriesService
{
	public async Task<Block[]> GetAllAsync(CancellationToken ct = default)
	{
		var query =
			from block in context.Blocks
			select block;

		return await query.ToArrayAsync(ct);
	}

	public async Task<Block[]> GetWithParentsAsync(int blockId, CancellationToken ct = default)
	{
		var cte = context.GetCte<Block>(nested =>
		{
			var baseQuery = context.Blocks.Where(x => x.Id == blockId);
			var recursiveQuery =
				from block in context.Blocks
				from child in nested.InnerJoin(x => x.ParentId == block.Id)
				select block;

			return baseQuery.Concat(recursiveQuery);
		});

		return await cte.ToArrayAsync(ct);
	}

	public async Task<BlockNestedTagInfo[]> GetNestedTagsAsync(IEnumerable<int> blocksId, CancellationToken ct = default)
	{
		var query =
			from relation in context.BlockTags
			from tag in context.Tags.AsSimpleInfo(context.Sources).LeftJoin(x => x.Id == relation.TagId)
			where blocksId.Contains(relation.BlockId)
			select new BlockNestedTagInfo
			{
				BlockId = relation.BlockId,
				LocalName = relation.Name,
				RelationType = relation.Relation,
				TagId = relation.TagId,
				Tag = tag,
			};

		return await query.ToArrayAsync(ct);
	}

	public async Task<BlockSimpleInfo[]> GetNestedBlocksAsync(int id, CancellationToken ct)
	{
		var query =
			from block in context.Blocks.AsSimpleInfo()
			where block.ParentBlockId == id
			select block;

		return await query.ToArrayAsync(ct);
	}

	public async Task<BlockPropertyInfo[]> GetPropertiesAsync(int id, CancellationToken ct)
	{
		var query =
			from property in context.BlockProperties
			where property.BlockId == id
			select new BlockPropertyInfo
			{
				Id = property.Id,
				Name = property.Name,
				Type = property.Type,
				Value = property.Value,
			};

		return await query.ToArrayAsync(ct);
	}
}
