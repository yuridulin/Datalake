using Datalake.InventoryService.Application.Repositories;
using Datalake.InventoryService.Domain.Entities;
using Datalake.PrivateApi.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.InventoryService.Infrastructure.Database.Repositories;

[Scoped]
public class SettingsRepository(InventoryEfContext context) : ISettingsRepository
{
	public async Task AddAsync(SettingsEntity settings, CancellationToken ct = default)
	{
		await context.Settings.AddAsync(settings, ct);
	}

	public async Task<SettingsEntity?> GetAsync(CancellationToken ct = default)
	{
		return await context.Settings.FirstOrDefaultAsync(ct);
	}

	public Task UpdateAsync(SettingsEntity entity, CancellationToken ct = default)
	{
		context.Settings.Update(entity);
		return Task.CompletedTask;
	}
}
