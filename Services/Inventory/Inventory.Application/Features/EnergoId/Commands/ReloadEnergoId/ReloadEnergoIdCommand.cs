using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Domain.Entities;

namespace Datalake.Inventory.Application.Features.EnergoId.Commands.ReloadEnergoId;

public record ReloadEnergoIdCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }
}
