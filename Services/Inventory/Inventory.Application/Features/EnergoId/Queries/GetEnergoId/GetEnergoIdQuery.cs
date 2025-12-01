using Datalake.Contracts.Models.Users;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.EnergoId.Queries.GetEnergoId;

public record GetEnergoIdQuery : IQueryRequest<List<UserEnergoIdInfo>>, IWithUserAccess
{
	public required UserAccessValue User { get; init; }
}
