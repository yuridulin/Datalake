using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Domain.Entities;
using Datalake.Inventory.Api.Models.Users;

namespace Datalake.Inventory.Application.Features.EnergoId.Queries.GetEnergoId;

public record GetEnergoIdQuery : IQueryRequest<IEnumerable<UserEnergoIdInfo>>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }
}
