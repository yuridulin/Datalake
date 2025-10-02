using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Domain.Entities;
using Datalake.Inventory.Api.Models.Users;

namespace Datalake.Inventory.Application.Features.Users.Queries.GetUserWithDetails;

public record GetUserWithDetailsQuery : IQueryRequest<UserDetailInfo>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public required Guid Guid { get; init; }
}
