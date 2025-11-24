using Datalake.Contracts.Models.Blocks;
using Datalake.Domain.Entities;

namespace Datalake.Inventory.Application.Queries;

/// <summary>
/// Запросы, связанные с блоками
/// </summary>
public interface IBlocksQueriesService
{
	/// <summary>
	/// Запрос информации о блоках
	/// </summary>
	Task<Block[]> GetAllAsync(CancellationToken ct = default);

	/// <summary>
	/// Запрос информации о блоке и его родителях
	/// </summary>
	Task<Block[]> GetWithParentsAsync(int blockId, CancellationToken ct = default);

	/// <summary>
	/// Запрос списка закрепленных за блоками тегов
	/// </summary>
	Task<BlockNestedTagInfo[]> GetNestedTagsAsync(IEnumerable<int> blocksId, CancellationToken ct = default);

	Task<BlockSimpleInfo[]> GetNestedBlocksAsync(int id, CancellationToken ct);

	Task<BlockPropertyInfo[]> GetPropertiesAsync(int id, CancellationToken ct);
}