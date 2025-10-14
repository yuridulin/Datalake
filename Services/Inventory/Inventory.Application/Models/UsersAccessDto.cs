using Datalake.Domain.ValueObjects;

namespace Datalake.Inventory.Application.Models;

public record UsersAccessDto
{
	public required long Version { get; init; }

	public required Dictionary<Guid, UserAccessValue> UserAccessEntities { get; init; }
}
