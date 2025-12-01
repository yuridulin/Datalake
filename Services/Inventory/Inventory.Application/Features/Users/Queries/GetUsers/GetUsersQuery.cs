using Datalake.Contracts.Models.Users;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery : IQueryRequest<List<UserInfo>>, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public required Guid? UserGuid { get; init; }
}
