using Datalake.Inventory.Application.Interfaces;
using System.Collections.Immutable;

namespace Datalake.Inventory.Infrastructure.InMemory.EnergoId;

/// <summary>
/// Состояние пользователей EnergoId
/// </summary>
public record class EnergoIdState : IEnergoIdState
{
	private EnergoIdState()
	{
		Users = [];
		UsersByGuid = ImmutableDictionary<Guid, Domain.Entities.EnergoId>.Empty;
	}

	public EnergoIdState(IEnumerable<Domain.Entities.EnergoId> data)
	{
		Users = data.ToImmutableList();
		UsersByGuid = Users.ToImmutableDictionary(x => x.Guid);
	}

	public static EnergoIdState Empty => new();

	/// <summary>
	/// Список пользователей
	/// </summary>
	public ImmutableList<Domain.Entities.EnergoId> Users { get; init; }

	/// <summary>
	/// Список пользователей, сопоставленный с идентификаторами
	/// </summary>
	public ImmutableDictionary<Guid, Domain.Entities.EnergoId> UsersByGuid { get; init; }
}
