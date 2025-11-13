using Datalake.Contracts.Public.Models.Users;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Users.Queries.GetUserWithDetails;

public record GetUserWithDetailsQuery : IQueryRequest<UserInfo>, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public required Guid Guid { get; init; }
}
