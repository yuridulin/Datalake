using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Models.UserGroups;

namespace Datalake.InventoryService.Application.Features.UserGroups.Queries.GetUserGroup;

public record GetUserGroupQuery : IQueryRequest<UserGroupInfo>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public required Guid Guid { get; init; }
}
