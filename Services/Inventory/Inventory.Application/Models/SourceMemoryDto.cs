using Datalake.Domain.Entities;
using Datalake.Domain.Interfaces;

namespace Datalake.Inventory.Application.Models;

public record SourceMemoryDto : IWithIdentityKey
{
	public required int Id { get; init; }

	public static SourceMemoryDto FromEntity(Source source)
	{
		return new()
		{
			Id = source.Id,
		};
	}
}