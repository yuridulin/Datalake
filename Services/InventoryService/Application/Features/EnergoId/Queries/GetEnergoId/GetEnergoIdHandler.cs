using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Queries;
using Datalake.PublicApi.Models.Users;

namespace Datalake.InventoryService.Application.Features.EnergoId.Queries.GetEnergoIdUsers;

public interface IGetEnergoIdHandler : IQueryHandler<GetEnergoIdQuery, IEnumerable<UserEnergoIdInfo>> { }

public class GetEnergoIdHandler(
	IEnergoIdQueriesService energoIdQueriesService) : IGetEnergoIdHandler
{
	public async Task<IEnumerable<UserEnergoIdInfo>> HandleAsync(GetEnergoIdQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoGlobalAccess(PublicApi.Enums.AccessType.Manager);

		var data = await energoIdQueriesService.GetAsync(ct);

		return data;
	}
}
