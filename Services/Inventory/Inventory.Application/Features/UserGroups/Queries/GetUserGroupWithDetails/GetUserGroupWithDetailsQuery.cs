using Datalake.Inventory.Api.Models.UserGroups;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroupWithDetails;

public class GetUserGroupWithDetailsQuery : IQueryRequest<UserGroupDetailedInfo>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public required Guid Guid { get; init; }
}
