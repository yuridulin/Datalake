using Datalake.Database.Views;
using System.Collections.Immutable;

namespace Datalake.Database.InMemory.Models;

/// <summary>
/// Состояние пользователей EnergoId
/// </summary>
public record class DatalakeEnergoIdState
{
	/// <summary>
	/// Список пользователей
	/// </summary>
	public required ImmutableList<EnergoIdUserView> Users { get; set; }

	/// <summary>
	/// Список пользователей, сопоставленный с идентификаторами
	/// </summary>
	public required ImmutableDictionary<Guid, EnergoIdUserView> UsersByGuid { get; set; }
}
