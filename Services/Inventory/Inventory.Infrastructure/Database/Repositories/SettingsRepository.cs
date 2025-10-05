using Datalake.Inventory.Application.Repositories;
using Datalake.Domain.Entities;
using Datalake.Shared.Application.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Inventory.Infrastructure.Database.Repositories;

[Scoped]
public class SettingsRepository(InventoryDbContext context) : ISettingsRepository
{
	public async Task AddAsync(Settings settings, CancellationToken ct = default)
	{
		await context.Settings.AddAsync(settings, ct);
	}

	public async Task<Settings?> GetAsync(CancellationToken ct = default)
	{
		return await context.Settings.FirstOrDefaultAsync(ct);
	}

	public Task UpdateAsync(Settings entity, CancellationToken ct = default)
	{
		context.Settings.Update(entity);
		return Task.CompletedTask;
	}
}
