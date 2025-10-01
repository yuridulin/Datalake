using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Models.Users;

namespace Datalake.InventoryService.Application.Features.EnergoId.Queries.GetEnergoIdUsers;

public record GetEnergoIdQuery : IQueryRequest<IEnumerable<UserEnergoIdInfo>>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }
}
