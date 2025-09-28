using Datalake.InventoryService.Application.Interfaces;
using Datalake.PublicApi.Models.Blocks;

namespace Datalake.InventoryService.Application.Features.Blocks.Queries.BlocksTree;

/// <summary>
/// Запрос информации о блоках со списками тегов IGetBlocksTreeQuery
/// </summary>
public interface IGetBlocksTreeQuery : IQueryHandler<GetBlocksTreeQuery, IEnumerable<BlockTreeInfo>>
{
}
