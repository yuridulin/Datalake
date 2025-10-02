using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Domain.Entities;

namespace Datalake.Inventory.Application.Features.Cache.Commands.ReloadCache;

public record ReloadCacheCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }
}
