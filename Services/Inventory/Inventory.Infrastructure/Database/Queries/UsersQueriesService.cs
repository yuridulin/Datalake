using Datalake.Contracts.Models;
using Datalake.Contracts.Models.UserGroups;
using Datalake.Contracts.Models.Users;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Datalake.Inventory.Infrastructure.Database.Queries;

public class UsersQueriesService(
	InventoryDbContext context,
	ILogger<UsersQueriesService> logger) : IUsersQueriesService
{
	public async Task<IEnumerable<UserInfo>> GetAsync(CancellationToken ct = default)
	{
		return await QueryUsersInfo()
			.ToArrayAsync(ct);
	}

	public async Task<UserInfo?> GetByGuidAsync(Guid userGuid, CancellationToken ct = default)
	{
		var query = QueryUsersInfo()
			.Where(user => user.Guid == userGuid);

		return await query.FirstOrDefaultAsync(cancellationToken: ct);
	}

	private IQueryable<UserInfo> QueryUsersInfo()
	{
		return
			from user in context.Users
			where !user.IsDeleted
			join energoUser in context.EnergoIdView on user.Guid equals energoUser.Guid into eu
			from energoUser in eu.DefaultIfEmpty()
			let userGlobalRule = user.AccessRules.OrderBy(r => r.Id).FirstOrDefault()
			let groupRelations = user.GroupsRelations
				.Where(gr => gr.UserGroup != null && !gr.UserGroup.IsDeleted)
				.Select(gr => new
				{
					Group = gr.UserGroup!,
					GroupGlobalRule = gr.UserGroup!.AccessRules.OrderBy(r => r.Id).FirstOrDefault()
				})
				.ToList()
			select new UserInfo
			{
				Guid = user.Guid,
				FullName = user.Type == UserType.Local
					? (user.FullName == null ? (user.Login == null ? string.Empty : user.Login!) : user.FullName!)
					: $"{energoUser.LastName} {energoUser.FirstName} {energoUser.MiddleName}",
				AccessType = userGlobalRule != null ? userGlobalRule.AccessType : AccessType.None,
				AccessRule = userGlobalRule != null
						? new AccessRuleInfo(userGlobalRule.Id, userGlobalRule.AccessType)
						: new AccessRuleInfo(0, AccessType.None),
				Login = user.Login,
				Type = user.Type,
				UserGroups = groupRelations.Select(gr => new UserGroupSimpleInfo
				{
					Guid = gr.Group.Guid,
					Name = gr.Group.Name,
					AccessRule = gr.GroupGlobalRule != null
						? new AccessRuleInfo(gr.GroupGlobalRule.Id, gr.GroupGlobalRule.AccessType)
						: new AccessRuleInfo(0, AccessType.None)
				}).ToList()
			};
	}
}
