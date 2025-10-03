using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Application.Entities;

namespace Datalake.Inventory.Application.Features.Cache.Commands.ReloadCache;

public record ReloadCacheCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }
}
