using Datalake.Contracts.Models.Blocks;
using Datalake.Inventory.Application.Features.Blocks.Queries.GetBlocksWithTags;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Queries.GetBlocksTree;

public interface IGetBlocksTreeHandler : IQueryHandler<GetBlocksTreeQuery, List<BlockTreeInfo>> { }

public class GetBlocksTreeHandler(
	IGetBlocksWithTagsHandler getHandler) : IGetBlocksTreeHandler
{
	public async Task<List<BlockTreeInfo>> HandleAsync(GetBlocksTreeQuery query, CancellationToken ct = default)
	{
		var blocksWithTags = await getHandler.HandleAsync(new() { User = query.User, }, ct);

		return BuildTree(blocksWithTags);
	}

	public static List<BlockTreeInfo> BuildTree(List<BlockWithTagsInfo> flatArray)
	{
		var lookup = flatArray.ToLookup(b => b.ParentBlockId);

		List<BlockTreeInfo> BuildChildren(int? parentId)
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
				.ToList();
		}

		return BuildChildren(null);
	}
}
