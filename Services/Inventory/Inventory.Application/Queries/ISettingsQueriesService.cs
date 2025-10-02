using Datalake.Inventory.Api.Models.Settings;

namespace Datalake.Inventory.Application.Queries;

public interface ISettingsQueriesService
{
	Task<SettingsInfo?> GetAsync(
		CancellationToken ct = default);
}
