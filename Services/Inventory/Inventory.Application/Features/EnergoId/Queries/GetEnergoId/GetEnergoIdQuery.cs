using Datalake.Domain.ValueObjects;
using Datalake.Inventory.Api.Models.Users;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.EnergoId.Queries.GetEnergoId;

public record GetEnergoIdQuery : IQueryRequest<IEnumerable<UserEnergoIdInfo>>, IWithUserAccess
{
	public required UserAccessValue User { get; init; }
}
