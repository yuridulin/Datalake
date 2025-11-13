using Datalake.Contracts.Public.Models.Settings;

namespace Datalake.Inventory.Application.Queries;

public interface ISettingsQueriesService
{
	Task<SettingsInfo?> GetAsync(
		CancellationToken ct = default);
}
