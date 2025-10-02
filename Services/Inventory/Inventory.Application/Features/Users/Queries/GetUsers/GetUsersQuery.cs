using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Domain.Entities;
using Datalake.Inventory.Api.Models.Users;

namespace Datalake.Inventory.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery : IQueryRequest<IEnumerable<UserInfo>>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }
}
