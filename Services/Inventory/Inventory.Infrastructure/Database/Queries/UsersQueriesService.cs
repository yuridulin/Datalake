using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Api.Models.UserGroups;
using Datalake.Inventory.Api.Models.Users;
using Datalake.Inventory.Application.Queries;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Datalake.Inventory.Infrastructure.Database.Queries;

public class UsersQueriesService(InventoryDbContext context) : IUsersQueriesService
{
	public async Task<IEnumerable<UserInfo>> GetAsync(CancellationToken ct = default)
	{
		return await QueryUsersInfo()
			.ToArrayAsync(ct);
	}

	public async Task<UserInfo?> GetByGuidAsync(Guid userGuid, CancellationToken ct = default)
	{
		return await QueryUsersInfo()
			.FirstOrDefaultAsync(user => user.Guid == userGuid, cancellationToken: ct);
	}

	private IQueryable<UserInfo> QueryUsersInfo()
	{
		return context.Users
			.Where(x => !x.IsDeleted)
			.AsNoTracking()
			.Select(x => new
			{
				User = x,
				UserGlobalRule = x.AccessRules.FirstOrDefault(),
				UserEnergoId = x.EnergoId,
				GroupRelations = x.GroupsRelations.Where(x => x.UserGroup != null && !x.UserGroup.IsDeleted).Select(r => new
				{
					Group = r.UserGroup,
					GroupGlobalRule = r.UserGroup.AccessRules.FirstOrDefault(),
				}),
			})
			.Select(x => new UserInfo
			{
				Guid = x.User.Guid,
				FullName = x.UserEnergoId == null ? $"{x.User.FullName}" : $"{x.UserEnergoId.LastName} {x.UserEnergoId.FirstName} {x.UserEnergoId.MiddleName}",
				AccessType = x.UserGlobalRule == null ? AccessType.None : x.UserGlobalRule.AccessType,
				AccessRule = x.UserGlobalRule == null ? new(0, AccessType.None) : new(x.UserGlobalRule.Id, x.UserGlobalRule.AccessType),
				Login = x.User.Login,
				Type = x.User.Type,
				UserGroups = x.GroupRelations
					.Select(r => new UserGroupSimpleInfo
					{
						Guid = r.Group.Guid,
						Name = r.Group.Name,
						AccessRule = r.GroupGlobalRule == null ? new(0, AccessType.None) : new(r.GroupGlobalRule.Id, r.GroupGlobalRule.AccessType),
					})
			});
	}
}
