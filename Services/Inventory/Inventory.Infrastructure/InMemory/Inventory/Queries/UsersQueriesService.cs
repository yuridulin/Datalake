using Datalake.Inventory.Api.Models.UserGroups;
using Datalake.Inventory.Api.Models.Users;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Exceptions;
using System.Data;

namespace Datalake.Inventory.Infrastructure.InMemory.Inventory.Queries;

public class UsersQueriesService(IInventoryCache inventoryCache) : IUsersQueriesService
{
	public Task<IEnumerable<UserInfo>> GetAsync(CancellationToken ct = default)
	{
		var state = inventoryCache.State;
		var globalAccessRules = state.AccessRules.Where(rule => rule.IsGlobal);

		var groups = state.ActiveUserGroups
			.Join(globalAccessRules, userGroup => userGroup.Guid, rule => rule.UserGroupGuid, (userGroup, groupRule) => new { userGroup, groupRule })
			.Select(x => new UserGroupSimpleInfo
			{
				Guid = x.userGroup.Guid,
				Name = x.userGroup.Name,
				AccessRule = new(x.groupRule.Id, x.groupRule.AccessType),
			})
			.ToDictionary(x => x.Guid);

		var data = state.ActiveUsers
			.Join(
				state.AccessRules.Where(x => x.IsGlobal && x.UserGuid.HasValue),
				x => x.Guid,
				x => x.UserGuid!.Value,
				(u, r) => new UserInfo
				{
					Login = u.Login,
					Guid = u.Guid,
					Type = u.Type,
					FullName = u.FullName,
					EnergoIdGuid = u.EnergoIdGuid,
					AccessType = r.AccessType,
					AccessRule = new(r.Id, r.AccessType),
					UserGroups = state.UserGroupRelations
						.Where(x => x.UserGuid == u.Guid)
						.Select(x => groups.TryGetValue(x.UserGroupGuid, out var group) ? group : null)
						.Where(x => x != null)
						.ToArray()!,
				});

		return Task.FromResult(data);
	}

	public Task<UserDetailInfo?> GetWithDetailsAsync(Guid userGuid, CancellationToken ct = default)
	{
		var state = inventoryCache.State;

		if (!state.ActiveUsersByGuid.TryGetValue(userGuid, out var user))
			return Task.FromResult<UserDetailInfo?>(null);

		var globalAccessRule = state.AccessRules.FirstOrDefault(x => x.IsGlobal && x.UserGuid == user.Guid)
			?? throw new InfrastructureException($"У пользователя {user.Guid} не найдено глобальное правило доступа");

		var data = new UserDetailInfo
		{
			Login = user.Login,
			Guid = user.Guid,
			Type = user.Type,
			FullName = user.FullName ?? string.Empty,
			EnergoIdGuid = user.EnergoIdGuid,
			UserGroups = state.UserGroupRelations
				.Where(x => x.UserGuid == user.Guid)
				.Select(x => state.ActiveUserGroupsByGuid.TryGetValue(x.UserGroupGuid, out var ug)
				? new UserGroupSimpleInfo
				{
					Guid = ug.Guid,
					Name = ug.Name,
				}
				: null)
				.Where(x => x != null)
				.ToArray()!,
			Hash = user.PasswordHash?.Value,
			StaticHost = user.StaticHost,
			AccessType = globalAccessRule.AccessType,
			AccessRule = new(globalAccessRule.Id, globalAccessRule.AccessType),
		};

		return Task.FromResult<UserDetailInfo?>(data);
	}
}
