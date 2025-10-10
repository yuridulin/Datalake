using Datalake.Domain.Entities;
using Datalake.Domain.Interfaces;

namespace Datalake.Inventory.Application.Models;

public record BlockMemoryDto : IWithIdentityKey
{
	public required int Id { get; init; }

	public required int? ParentId { get; init; }

	public static BlockMemoryDto FromEntity(Block block)
	{
		return new()
		{
			Id = block.Id,
			ParentId = block.ParentId,
		};
	}
}
