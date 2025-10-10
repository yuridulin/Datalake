using Datalake.Domain.Entities;
using Datalake.Domain.Interfaces;

namespace Datalake.Inventory.Application.Models;

public class TagMemoryDto : IWithIdentityKey
{
	public required int Id { get; init; }

	public static TagMemoryDto FromEntity(Tag tag)
	{
		return new() { Id = tag.Id };
	}
}
