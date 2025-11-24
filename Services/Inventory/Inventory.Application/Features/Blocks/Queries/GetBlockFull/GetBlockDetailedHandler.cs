using Datalake.Contracts.Models;
using Datalake.Contracts.Models.AccessRules;
using Datalake.Contracts.Models.Blocks;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Queries.GetBlockFull;

/// <summary>
/// Запрос информации о конкретном блоке со всеми его отношениями к другим объектам
/// </summary>
public interface IGetBlockDetailedHandler : IQueryHandler<GetBlockFullQuery, BlockDetailedInfo> { }

public class GetBlockDetailedHandler(
	IAccessRulesQueriesService accessRulesQueriesService,
	IBlocksQueriesService blocksQueriesService) : IGetBlockDetailedHandler
{
	public async Task<BlockDetailedInfo> HandleAsync(GetBlockFullQuery query, CancellationToken ct = default)
	{
		var blockWithParents = await blocksQueriesService.GetWithParentsAsync(query.BlockId, ct);
		if (blockWithParents.Length == 0)
			throw InventoryNotFoundException.NotFoundBlock(query.BlockId);

		var block = blockWithParents[0];
		var parents = blockWithParents.Skip(1);

		var tags = await blocksQueriesService.GetNestedTagsAsync([block.Id], ct);
		var childs = await blocksQueriesService.GetNestedBlocksAsync(block.Id, ct);
		var rules = await accessRulesQueriesService.GetAsync(blockId: block.Id, ct: ct);
		var properties = await blocksQueriesService.GetPropertiesAsync(block.Id, ct);

		var result = new BlockDetailedInfo
		{
			Id = block.Id,
			Guid = block.GlobalId,
			Name = block.Name,
			Adults = parents
				.Select(x => new BlockSimpleInfo
				{
					Id = x.Id,
					Guid = x.GlobalId,
					Name = x.Name,
					Description = x.Description,
					ParentBlockId = block.Id,
					AccessRule = AccessRuleInfo.FromRule(query.User.GetAccessToBlock(x.Id)),
				})
				.ToArray(),
			Tags = tags,
			Children = childs,
			AccessRules = rules
				.Select(x => new AccessRulesForObjectInfo
				{
					Id = x.Id,
					IsGlobal = x.IsGlobal,
					AccessType = x.AccessType,
					User = x.User,
					UserGroup = x.UserGroup,
				})
				.ToArray(),
			Description = block.Description,
			ParentBlockId = block.ParentId,
			Properties = properties,
			AccessRule = AccessRuleInfo.FromRule(query.User.GetAccessToBlock(block.Id)),
		};

		return result;
	}
}
