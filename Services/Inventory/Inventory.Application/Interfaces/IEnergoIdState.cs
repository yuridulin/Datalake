using Datalake.Domain.Entities;
using System.Collections.Immutable;

namespace Datalake.Inventory.Application.Interfaces;

public interface IEnergoIdState
{
	ImmutableList<EnergoId> Users { get; }

	ImmutableDictionary<Guid, EnergoId> UsersByGuid { get; }
}