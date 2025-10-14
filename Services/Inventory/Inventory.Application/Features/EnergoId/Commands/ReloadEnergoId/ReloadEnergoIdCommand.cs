using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.EnergoId.Commands.ReloadEnergoId;

public record ReloadEnergoIdCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessValue User { get; init; }
}
