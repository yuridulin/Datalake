using Datalake.Contracts.Models.Settings;
using Datalake.Inventory.Application.Queries;
using LinqToDB;

namespace Datalake.Inventory.Infrastructure.Database.Queries;

public class SettingsQueriesService(InventoryDbLinqContext context) : ISettingsQueriesService
{
	public async Task<SettingsInfo?> GetAsync(CancellationToken ct = default)
	{
		return await context.Settings
			.Select(x => new SettingsInfo
			{
				InstanceName = x.InstanceName,
				EnergoIdApi = x.EnergoIdApi,
				EnergoIdClient = x.KeycloakClient,
				EnergoIdHost = x.KeycloakHost,
			})
			.FirstOrDefaultAsync(ct);
	}
}
