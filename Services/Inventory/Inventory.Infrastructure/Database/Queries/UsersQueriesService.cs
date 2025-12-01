using Datalake.Contracts.Models.UserGroups;
using Datalake.Contracts.Models.Users;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Queries;
using Datalake.Inventory.Infrastructure.Database.Extensions;
using LinqToDB;
using System.Data;

namespace Datalake.Inventory.Infrastructure.Database.Queries;

public class UsersQueriesService(InventoryDbLinqContext context) : IUsersQueriesService
{
	public async Task<List<UserInfo>> GetAsync(CancellationToken ct = default)
	{
		return await QueryUsersInfo()
			.ToListAsync(ct);
	}

	public async Task<UserInfo?> GetByGuidAsync(Guid userGuid, CancellationToken ct = default)
	{
		var query = QueryUsersInfo()
			.Where(user => user.Guid == userGuid);

		return await query.FirstOrDefaultAsync(ct);
	}

	public async Task<List<UserGroupSimpleInfo>> GetGroupsWithMemberAsync(Guid userGuid, CancellationToken ct)
	{
		var query =
			from relation in context.UserGroupRelations
			from userGroup in context.UserGroups.AsSimpleInfo().InnerJoin(x => x.Guid == relation.UserGroupGuid)
			select userGroup;

		return await query.ToListAsync(ct);
	}

	private IQueryable<UserInfo> QueryUsersInfo()
	{
		return
			from user in context.Users
			from energo in context.EnergoId.LeftJoin(x => x.Guid == user.Guid)
			from rule in context.CalculatedAccessRules.LeftJoin(x => x.UserGuid == user.Guid && x.IsGlobal)
			select new UserInfo
			{
				Guid = user.Guid,
				Type = user.Type,
				FullName = user.Type == UserType.EnergoId
					? (energo == null ? "Имя не найдено в EnergoId" : $"{energo.LastName} {energo.FirstName} {energo.MiddleName}")
					: (user.FullName ?? user.Login ?? "Имя не найдено в системе"),
				AccessType = rule == null ? AccessType.None : rule.AccessType,
				Login = user.Type == UserType.Local ? user.Login : null,
			};
	}
}
