using Datalake.InventoryService.Domain.Entities;
using System.Collections.Immutable;

namespace Datalake.InventoryService.Infrastructure.Cache.EnergoId;

/// <summary>
/// Состояние пользователей EnergoId
/// </summary>
public record class EnergoIdState
{
	/// <summary>
	/// Список пользователей
	/// </summary>
	public required ImmutableList<EnergoIdEntity> Users { get; set; }

	/// <summary>
	/// Список пользователей, сопоставленный с идентификаторами
	/// </summary>
	public required ImmutableDictionary<Guid, EnergoIdEntity> UsersByGuid { get; set; }
}
