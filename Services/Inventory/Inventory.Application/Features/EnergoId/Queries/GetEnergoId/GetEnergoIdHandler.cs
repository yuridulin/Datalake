using Datalake.Contracts.Models.Users;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.EnergoId.Queries.GetEnergoId;

public interface IGetEnergoIdHandler : IQueryHandler<GetEnergoIdQuery, IEnumerable<UserEnergoIdInfo>> { }

public class GetEnergoIdHandler(
	IEnergoIdQueriesService energoIdQueriesService) : IGetEnergoIdHandler
{
	public async Task<IEnumerable<UserEnergoIdInfo>> HandleAsync(GetEnergoIdQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoGlobalAccess(AccessType.Manager);

		var data = await energoIdQueriesService.GetAsync(ct);

		return data;
	}
}
