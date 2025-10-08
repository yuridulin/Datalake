using Datalake.Inventory.Application.Interfaces.InMemory;
using System.Collections.Immutable;

namespace Datalake.Inventory.Infrastructure.Cache.EnergoId;

/// <summary>
/// Состояние пользователей EnergoId
/// </summary>
public record class EnergoIdState : IEnergoIdCacheState
{
	/// <summary>
	/// Список пользователей
	/// </summary>
	public required ImmutableList<Domain.Entities.EnergoId> Users { get; set; }

	/// <summary>
	/// Список пользователей, сопоставленный с идентификаторами
	/// </summary>
	public required ImmutableDictionary<Guid, Domain.Entities.EnergoId> UsersByGuid { get; set; }
}
