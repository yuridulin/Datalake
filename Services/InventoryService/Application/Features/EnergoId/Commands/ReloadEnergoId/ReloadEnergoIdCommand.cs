using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.EnergoId.Commands.ReloadEnergoId;

public record ReloadEnergoIdCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }
}
