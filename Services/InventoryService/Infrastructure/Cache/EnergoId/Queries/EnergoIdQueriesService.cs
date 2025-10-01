using Datalake.InventoryService.Application.Interfaces.InMemory;
using Datalake.InventoryService.Application.Queries;
using Datalake.PublicApi.Models.Users;

namespace Datalake.InventoryService.Infrastructure.Cache.EnergoId.Queries;

public class EnergoIdQueriesService(
	IInventoryCache inventoryCache,
	IEnergoIdCache energoIdCache) : IEnergoIdQueriesService
{
	public Task<IEnumerable<UserEnergoIdInfo>> GetAsync(CancellationToken ct = default)
	{
		var inventory = inventoryCache.State;
		var energoId = energoIdCache.State;

		var mappedToInventory = inventory.ActiveUsers
			.Where(x => x.EnergoIdGuid != null)
			.ToDictionary(x => x.EnergoIdGuid!.Value, x => x.Guid);

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
