using Datalake.Inventory.Api.Models.Users;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.EnergoId.Queries.GetEnergoId;

public record GetEnergoIdQuery : IQueryRequest<IEnumerable<UserEnergoIdInfo>>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }
}
