using Datalake.Inventory.Api.Models.Users;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery : IQueryRequest<IEnumerable<UserInfo>>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }
}
