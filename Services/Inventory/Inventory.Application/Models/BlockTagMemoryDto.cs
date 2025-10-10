using Datalake.Domain.Entities;

namespace Datalake.Inventory.Application.Models;

public record BlockTagMemoryDto
{
	public required int BlockId { get; init; }

	public required int? TagId { get; init; }

	public static BlockTagMemoryDto FromEntity(BlockTag blockTag)
	{
		return new()
		{
			TagId = blockTag.TagId,
			BlockId = blockTag.BlockId,
		};
	}
}
