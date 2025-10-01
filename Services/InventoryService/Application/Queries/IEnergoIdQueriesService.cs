using Datalake.PublicApi.Models.Users;

namespace Datalake.InventoryService.Application.Queries;

public interface IEnergoIdQueriesService
{
	Task<IEnumerable<UserEnergoIdInfo>> GetAsync(CancellationToken ct = default);
}
