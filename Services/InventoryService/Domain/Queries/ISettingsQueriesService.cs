using Datalake.PublicApi.Models.Settings;

namespace Datalake.InventoryService.Domain.Queries;

public interface ISettingsQueriesService
{
	Task<SettingsInfo?> GetAsync(CancellationToken ct = default);
}
