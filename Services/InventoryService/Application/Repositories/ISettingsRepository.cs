using Datalake.InventoryService.Domain.Entities;

namespace Datalake.InventoryService.Application.Repositories;

public interface ISettingsRepository
{
	Task<SettingsEntity?> GetAsync(CancellationToken ct = default);

	Task AddAsync(SettingsEntity settings, CancellationToken ct = default);

	Task UpdateAsync(SettingsEntity entity, CancellationToken ct = default);
}
