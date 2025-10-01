using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.Cache.Commands.ReloadCache;

public record ReloadCacheCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }
}
