using Datalake.Contracts.Models.Users;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Queries;

namespace Datalake.Inventory.Infrastructure.InMemory.EnergoId.Queries;

public class EnergoIdQueriesService(
	IInventoryStore inventoryCache,
	IEnergoIdStore energoIdCache) : IEnergoIdQueriesService
{
	public Task<List<UserEnergoIdInfo>> GetAsync(CancellationToken ct = default)
	{
		var inventory = inventoryCache.State;
		var energoId = energoIdCache.State;

		var mappedToInventory = inventory.Users
			.Where(x => x.Value.IsEnergoId)
			.ToDictionary(x => x.Value.Guid, x => x.Key);

		var users = energoId.Users
			.Select(x => new UserEnergoIdInfo
			{
				EnergoIdGuid = x.Guid,
				FullName = x.GetFullName(),
				Email = x.Email ?? "нет адреса",
				UserGuid = mappedToInventory.TryGetValue(x.Guid, out var userGuid) ? userGuid : null,
			})
			.ToList();

		return Task.FromResult(users);
	}
}
