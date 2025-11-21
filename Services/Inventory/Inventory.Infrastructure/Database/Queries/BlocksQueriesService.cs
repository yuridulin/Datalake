using Datalake.Contracts.Models.Blocks;
using Datalake.Inventory.Application.Queries;
using Datalake.Inventory.Infrastructure.Database.Extensions;
using LinqToDB;

namespace Datalake.Inventory.Infrastructure.Database.Queries;

public class BlocksQueriesService(InventoryDbLinqContext context) : IBlocksQueriesService
{
	public async Task<IEnumerable<BlockTreeInfo>> GetAsync(CancellationToken ct = default)
	{
		return await QueryBlockTree().ToArrayAsync(ct);
	}

	public async Task<BlockTreeInfo?> GetAsync(int blockId, CancellationToken ct = default)
	{
		return await QueryBlockTree().FirstOrDefaultAsync(x => x.Id == blockId, ct);
	}

	public async Task<BlockTreeInfo[]> GetWithParentsAsync(int blockId, CancellationToken ct = default)
	{
		var cte = context.GetCte<BlockTreeInfo>(nested =>
		{
			var baseQuery = QueryBlockTree().Where(x => x.Id == blockId);
			var recursiveQuery =
				from block in QueryBlockTree()
				from parent in nested.InnerJoin(x => x.Id == block.ParentBlockId)
				select parent;

			return baseQuery.Concat(recursiveQuery);
		});

		return await cte.ToArrayAsync(ct);
	}

	public async Task<BlockNestedTagInfo[]> GetBlockNestedTagsAsync(IEnumerable<int> blocksId, CancellationToken ct = default)
	{
		return await QueryBlockTags()
			.Where(x => blocksId.Contains(x.BlockId))
			.ToArrayAsync(ct);
	}

	internal IQueryable<BlockTreeInfo> QueryBlockTree()
	{
		return
			from block in context.Blocks
			select new BlockTreeInfo
			{
				Id = block.Id,
				Guid = block.GlobalId,
				Name = block.Name,
				ParentBlockId = block.ParentId,
			};
	}

	internal IQueryable<BlockNestedTagInfo> QueryBlockTags()
	{
		return
			from relation in context.BlockTags
			from tag in context.Tags.AsSimpleInfo(context.Sources).LeftJoin(x => x.Id == relation.TagId)
			select new BlockNestedTagInfo
			{
				BlockId = relation.BlockId,
				LocalName = relation.Name,
				RelationType = relation.Relation,
				TagId = relation.TagId,
				Tag = tag,
			};
	}
}
