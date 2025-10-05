using Datalake.Domain.Entities;
using System.Collections.Immutable;

namespace Datalake.Inventory.Application.Interfaces.InMemory;

public interface IEnergoIdCacheState
{
	ImmutableList<EnergoId> Users { get; }

	ImmutableDictionary<Guid, EnergoId> UsersByGuid { get; }
}