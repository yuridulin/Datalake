using Datalake.Domain.ValueObjects;
using Datalake.Inventory.Api.Models.UserGroups;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroupsTree;

public class GetUserGroupsTreeQuery : IQueryRequest<IEnumerable<UserGroupTreeInfo>>, IWithUserAccess
{
	public required UserAccessValue User { get; init; }
}
