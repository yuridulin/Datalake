using Datalake.Shared.Application.Entities;

namespace Datalake.Inventory.Application.Models;

public record UsersAccessDto
{
	public required long Version { get; init; }

	public required Dictionary<Guid, UserAccessValue> UserAccessEntities { get; init; }
}
