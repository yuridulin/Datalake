using Datalake.Contracts.Models.Blocks;

namespace Datalake.Inventory.Application.Queries;

/// <summary>
/// Запросы, связанные с блоками
/// </summary>
public interface IBlocksQueriesService
{
	/// <summary>
	/// Запрос информации о конкретном блоке со всеми его отношениями к другим объектам
	/// </summary>
	Task<BlockTreeInfo?> GetAsync(int blockId, CancellationToken ct = default);

	/// <summary>
	/// Запрос информации о блоках со списками тегов
	/// </summary>
	Task<IEnumerable<BlockTreeInfo>> GetAsync(CancellationToken ct = default);

	/// <summary>
	/// Запрос информации о блоке и его родителях
	/// </summary>
	Task<BlockTreeInfo[]> GetWithParentsAsync(int blockId, CancellationToken ct = default);

	/// <summary>
	/// Запрос списка закрепленных за блоками тегов
	/// </summary>
	Task<BlockNestedTagInfo[]> GetBlockNestedTagsAsync(IEnumerable<int> blocksId, CancellationToken ct = default);
}