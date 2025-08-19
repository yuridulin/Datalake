using Datalake.Database.InMemory.Models;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.UserGroups;
using Datalake.PublicApi.Models.Users;
using System.Data;

namespace Datalake.Database.InMemory.Queries;

/// <summary>
/// Запросы, связанные с пользователями
/// </summary>
public static class UsersQueries
{
	/// <summary>
	/// Запрос полной информации о учетных записях, включая группы и права доступа
	/// </summary>
	/// <param name="state">Текущее состояние данных</param>
	public static IEnumerable<UserInfo> UsersInfo(this DatalakeDataState state)
	{
		var globalAccessRules = state.AccessRights.Where(rule => rule.IsGlobal);

		var userGroupsWithRules = state.UserGroups
			.Where(userGroup => !userGroup.IsDeleted)
			.Join(globalAccessRules, userGroup => userGroup.Guid, rule => rule.UserGroupGuid, (userGroup, groupRule) => new { userGroup, groupRule });

		return state.UsersDetailInfo()
			.Select(user => new UserInfo
			{
				Login = user.Login,
				Guid = user.Guid,
				Type = user.Type,
				FullName = user.FullName,
				EnergoIdGuid = user.EnergoIdGuid,
				UserGroups = user.UserGroups,
				AccessType = user.AccessType,
			});
	}

	/// <summary>
	/// Получение полной информации о учетных записях, включая группы, права доступа и данные для входа
	/// </summary>
	/// <param name="state">Текущее состояние данных</param>
	public static IEnumerable<UserDetailInfo> UsersDetailInfo(this DatalakeDataState state)
	{
		var globalAccessRules = state.AccessRights.Where(rule => rule.IsGlobal);

		var userGroupsWithRules = state.UserGroups
			.Where(userGroup => !userGroup.IsDeleted)
			.Join(globalAccessRules, userGroup => userGroup.Guid, rule => rule.UserGroupGuid, (userGroup, groupRule) => new { userGroup, groupRule });

		return state.Users
			.Where(user => !user.IsDeleted)
			.Join(
				globalAccessRules,
				user => user.Guid,
				rule => rule.UserGuid,
				(user, globalRule) =>
				{
					var userGroups = state.UserGroupRelations
						.Where(relation => relation.UserGuid == user.Guid)
						.Join(
							userGroupsWithRules,
							relation => relation.UserGroupGuid,
							userGroupBadge => userGroupBadge.userGroup.Guid,
							(_, userGroupBadge) => userGroupBadge)
						.ToArray();

					return new UserDetailInfo
					{
						Login = user.Login,
						Guid = user.Guid,
						Type = user.Type,
						FullName = user.FullName ?? string.Empty,
						EnergoIdGuid = user.EnergoIdGuid,
						UserGroups = userGroups
							.Select(badge => new UserGroupSimpleInfo
							{
								Guid = badge.userGroup.Guid,
								Name = badge.userGroup.Name,
							})
							.ToArray(),
						AccessType = userGroups
							.Select(badge => badge.groupRule.AccessType)
							.Append(globalRule.AccessType)
							.DefaultIfEmpty(AccessType.NoAccess)
							.Max(),
						Hash = user.PasswordHash,
						StaticHost = user.StaticHost,
					};
				});
	}
}
