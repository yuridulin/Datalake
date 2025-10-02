using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Domain.Entities;
using Datalake.Inventory.Api.Models.UserGroups;

namespace Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroupsTree;

public class GetUserGroupsTreeQuery : IQueryRequest<IEnumerable<UserGroupTreeInfo>>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }
}
