using Datalake.Inventory.Domain.Entities;
using System.Collections.Immutable;

namespace Datalake.Inventory.Application.Interfaces.InMemory;

public interface IEnergoIdCacheState
{
	ImmutableList<EnergoIdEntity> Users { get; }

	ImmutableDictionary<Guid, EnergoIdEntity> UsersByGuid { get; }
}