using Datalake.Inventory.Api.Models.Users;

namespace Datalake.Inventory.Application.Queries;

public interface IEnergoIdQueriesService
{
	Task<IEnumerable<UserEnergoIdInfo>> GetAsync(CancellationToken ct = default);
}
