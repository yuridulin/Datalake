using Datalake.Contracts.Models.Users;

namespace Datalake.Inventory.Application.Queries;

public interface IEnergoIdQueriesService
{
	Task<List<UserEnergoIdInfo>> GetAsync(CancellationToken ct = default);
}
