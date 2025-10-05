using Datalake.Domain.Entities;

namespace Datalake.Inventory.Application.Repositories;

public interface ISettingsRepository
{
	Task<Settings?> GetAsync(CancellationToken ct = default);

	Task AddAsync(Settings settings, CancellationToken ct = default);

	Task UpdateAsync(Settings entity, CancellationToken ct = default);
}
