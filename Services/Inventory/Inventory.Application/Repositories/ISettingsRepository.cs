using Datalake.Domain.Entities;

namespace Datalake.Inventory.Application.Repositories;

public interface ISettingsRepository
{
	Task<SettingsEntity?> GetAsync(CancellationToken ct = default);

	Task AddAsync(SettingsEntity settings, CancellationToken ct = default);

	Task UpdateAsync(SettingsEntity entity, CancellationToken ct = default);
}
