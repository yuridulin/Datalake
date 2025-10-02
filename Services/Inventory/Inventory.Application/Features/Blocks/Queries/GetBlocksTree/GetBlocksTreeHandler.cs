using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Queries;
using Datalake.Inventory.Api.Models.Blocks;
using Datalake.Shared.Domain.ValueObjects;

namespace Datalake.Inventory.Application.Features.Blocks.Queries.GetBlocksTree;

/// <summary>
/// Запрос информации о блоках со списками тегов IGetBlocksTreeQuery
/// </summary>
public interface IGetBlocksTreeHandler : IQueryHandler<GetBlocksTreeQuery, IEnumerable<BlockTreeInfo>> { }

public class GetBlocksTreeHandler(
	IBlocksQueriesService blocksQueriesService) : IGetBlocksTreeHandler
{
	public async Task<IEnumerable<BlockTreeInfo>> HandleAsync(GetBlocksTreeQuery query, CancellationToken ct = default)
	{
		var data = await blocksQueriesService.GetWithTagsAsync(ct);

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
