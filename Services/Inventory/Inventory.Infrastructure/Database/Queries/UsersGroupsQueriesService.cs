using Datalake.Contracts.Models.UserGroups;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Queries;
using Datalake.Inventory.Infrastructure.Database.Extensions;
using LinqToDB;

namespace Datalake.Inventory.Infrastructure.Database.Queries;

public class UsersGroupsQueriesService(InventoryDbLinqContext context) : IUsersGroupsQueriesService
{
	public async Task<List<UserGroupInfo>> GetAsync(
		CancellationToken ct = default)
	{
		return await QueryUserGroupInfo().ToListAsync(ct);
	}

	public async Task<UserGroupInfo?> GetAsync(
		Guid userGroupGuid,
		CancellationToken ct = default)
	{
		return await QueryUserGroupInfo().FirstOrDefaultAsync(x => x.Guid == userGroupGuid, ct);
	}

	public async Task<List<UserGroupSimpleInfo>> GetByParentGuidAsync(Guid userGroupGuid, CancellationToken ct)
	{
		var query =
			from userGroup in context.UserGroups
			where userGroup.ParentGuid == userGroupGuid
			select new UserGroupSimpleInfo
			{
				Guid = userGroup.Guid,
				Name = userGroup.Name,
			};

		return await query.ToListAsync(ct);
	}

	public async Task<List<UserGroupMemberInfo>> GetMembersAsync(Guid userGroupGuid, CancellationToken ct)
	{
		var query =
			from relation in context.UserGroupRelations
			from user in context.Users.AsSimpleInfo(context.EnergoId).LeftJoin(x => x.Guid == relation.UserGroupGuid)
			select new UserGroupMemberInfo
			{
				Guid = user == null ? relation.UserGroupGuid : user.Guid,
				FullName = user == null ? "Не удалось найти пользователя" : user.FullName,
				Type = user == null ? UserType.Local : user.Type,
				AccessType = relation.AccessType,
			};

		return await query.ToListAsync(ct);
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
