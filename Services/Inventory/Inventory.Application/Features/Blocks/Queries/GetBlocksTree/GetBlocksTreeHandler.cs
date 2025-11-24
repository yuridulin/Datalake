using Datalake.Contracts.Models.Blocks;
using Datalake.Inventory.Application.Features.Blocks.Queries.GetBlocksWithTags;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Queries.GetBlocksTree;

public interface IGetBlocksTreeHandler : IQueryHandler<GetBlocksTreeQuery, IEnumerable<BlockTreeInfo>> { }

public class GetBlocksTreeHandler(
	IGetBlocksWithTagsHandler getHandler) : IGetBlocksTreeHandler
{
	public async Task<IEnumerable<BlockTreeInfo>> HandleAsync(GetBlocksTreeQuery query, CancellationToken ct = default)
	{
		var blocksWithTags = await getHandler.HandleAsync(new() { User = query.User, }, ct);

		return BuildTree(blocksWithTags is BlockWithTagsInfo[] array ? array : blocksWithTags.ToArray());
	}

	public static BlockTreeInfo[] BuildTree(BlockWithTagsInfo[] flatArray)
	{
		var lookup = flatArray.ToLookup(b => b.ParentBlockId);

		BlockTreeInfo[] BuildChildren(int? parentId)
		{
			return lookup[parentId]
				.OrderBy(b => b.Name)
				.Select(b => new BlockTreeInfo
				{
					Id = b.Id,
					ParentBlockId = b.ParentBlockId,
					Guid = b.Guid,
					Name = b.Name,
					Description = b.Description,
					AccessRule = b.AccessRule,
					Tags = b.Tags,
					Children = BuildChildren(b.Id)
				})
				.ToArray();
		}

		return BuildChildren(null);
	}
}
