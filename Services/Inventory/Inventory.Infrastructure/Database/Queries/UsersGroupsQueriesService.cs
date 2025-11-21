using Datalake.Contracts.Models.UserGroups;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Queries;
using LinqToDB;

namespace Datalake.Inventory.Infrastructure.Database.Queries;

public class UsersGroupsQueriesService(InventoryDbLinqContext context) : IUsersGroupsQueriesService
{
	public async Task<IEnumerable<UserGroupInfo>> GetAsync(
		CancellationToken ct = default)
	{
		return await QueryUserGroupInfo().ToArrayAsync(ct);
	}

	public async Task<UserGroupInfo?> GetAsync(
		Guid userGroupGuid,
		CancellationToken ct = default)
	{
		return await QueryUserGroupInfo().FirstOrDefaultAsync(x => x.Guid == userGroupGuid, ct);
	}

	private IQueryable<UserGroupInfo> QueryUserGroupInfo()
	{
		return
			from userGroup in context.UserGroups
			from rule in context.CalculatedAccessRules.LeftJoin(x => x.UserGroupGuid == userGroup.Guid && x.IsGlobal)
			select new UserGroupInfo
			{
				Guid = userGroup.Guid,
				Name = userGroup.Name,
				Description = userGroup.Description,
				ParentGroupGuid = userGroup.ParentGuid,
				GlobalAccessType = rule == null ? AccessType.None : rule.AccessType,
			};
	}
}
