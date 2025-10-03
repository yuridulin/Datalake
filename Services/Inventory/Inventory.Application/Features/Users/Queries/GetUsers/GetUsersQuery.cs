using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Api.Models.Users;
using Datalake.Shared.Application.Entities;

namespace Datalake.Inventory.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery : IQueryRequest<IEnumerable<UserInfo>>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }
}
