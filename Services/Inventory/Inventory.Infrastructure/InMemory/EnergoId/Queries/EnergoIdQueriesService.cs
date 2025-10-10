using Datalake.Inventory.Api.Models.Users;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Queries;

namespace Datalake.Inventory.Infrastructure.InMemory.EnergoId.Queries;

public class EnergoIdQueriesService(
	IInventoryCache inventoryCache,
	IEnergoIdCache energoIdCache) : IEnergoIdQueriesService
{
	public Task<IEnumerable<UserEnergoIdInfo>> GetAsync(CancellationToken ct = default)
	{
		var inventory = inventoryCache.State;
		var energoId = energoIdCache.State;

		var mappedToInventory = inventory.Users
			.Where(x => x.Value.EnergoIdGuid != null)
			.ToDictionary(x => x.Value.EnergoIdGuid!.Value, x => x.Key);

		var users = energoId.Users
			.Select(x => new UserEnergoIdInfo
			{
				EnergoIdGuid = x.Guid,
				FullName = x.GetFullName(),
				Email = x.Email ?? "нет адреса",
				UserGuid = mappedToInventory.TryGetValue(x.Guid, out var userGuid) ? userGuid : null,
			})
			.ToArray();

		return Task.FromResult<IEnumerable<UserEnergoIdInfo>>(users);
	}
}
