using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Cache.Commands.ReloadCache;

public record ReloadCacheCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }
}
