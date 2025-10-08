using Datalake.Inventory.Api.Models.Settings;
using Datalake.Inventory.Application.Queries;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Inventory.Infrastructure.Database.Queries;

public class SettingsQueriesService(InventoryDbContext context) : ISettingsQueriesService
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
			.FirstOrDefaultAsync(cancellationToken: ct);
	}
}
