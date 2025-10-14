using Datalake.Domain.ValueObjects;
using Datalake.Inventory.Api.Models.Users;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery : IQueryRequest<IEnumerable<UserInfo>>, IWithUserAccess
{
	public required UserAccessValue User { get; init; }
}
