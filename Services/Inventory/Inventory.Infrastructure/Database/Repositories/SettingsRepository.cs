using Datalake.Inventory.Application.Repositories;
using Datalake.Inventory.Domain.Entities;
using Datalake.Shared.Application.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Inventory.Infrastructure.Database.Repositories;

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
