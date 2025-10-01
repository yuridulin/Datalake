using Datalake.PublicApi.Models.Settings;

namespace Datalake.InventoryService.Application.Queries;

public interface ISettingsQueriesService
{
	Task<SettingsInfo?> GetAsync(
		CancellationToken ct = default);
}
