using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Cache.Commands.ReloadCache;

public record ReloadCacheCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessValue User { get; init; }
}
