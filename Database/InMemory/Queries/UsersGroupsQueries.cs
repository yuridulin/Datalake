using Datalake.Database.Constants;
using Datalake.Database.InMemory.Models;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.AccessRights;
using Datalake.PublicApi.Models.Blocks;
using Datalake.PublicApi.Models.Sources;
using Datalake.PublicApi.Models.Tags;
using Datalake.PublicApi.Models.UserGroups;
using LinqToDB;

namespace Datalake.Database.InMemory.Queries;

/// <summary>
/// Запросы, связанные с группами пользователей
/// </summary>
public static class UsersGroupsQueries
{
	/// <summary>
	/// Запрос краткой информации о группах пользователей
	/// </summary>
	/// <param name="state">Текущее состояние данных</param>
	public static IEnumerable<UserGroupInfo> UserGroupsInfo(this DatalakeDataState state)
	{
		return state.UserGroups
			.Where(userGroup => !userGroup.IsDeleted)
			.Join(
				state.AccessRights.Where(rule => rule.IsGlobal && rule.UserGroupGuid.HasValue),
				userGroup => userGroup.Guid,
				rule => rule.UserGroupGuid,
				(userGroup, rule) => new UserGroupInfo
				{
					Guid = userGroup.Guid,
					Name = userGroup.Name,
					Description = userGroup.Description,
					ParentGroupGuid = userGroup.ParentGuid,
					GlobalAccessType = rule.AccessType,
				});
	}

	/// <summary>
	/// Запрос информации о группах пользователей с правами, подгруппами и участниками
	/// </summary>
	/// <param name="state">Текущее состояние данных</param>
	public static IEnumerable<UserGroupDetailedInfo> UserGroupsInfoWithDetails(this DatalakeDataState state)
	{
		var activeUserGroups = state.UserGroups.Where(userGroup => !userGroup.IsDeleted);

		return activeUserGroups
			.Join(
				state.AccessRights.Where(rule => rule.IsGlobal && rule.UserGroupGuid.HasValue),
				userGroup => userGroup.Guid,
				rule => rule.UserGroupGuid,
				(userGroup, rule) => new UserGroupDetailedInfo
				{
					Guid = userGroup.Guid,
					Name = userGroup.Name,
					Description = userGroup.Description,
					ParentGroupGuid = userGroup.ParentGuid,
					GlobalAccessType = rule.AccessType,
					Users = state.UserGroupRelations
						.Where(relation => relation.UserGroupGuid == userGroup.Guid)
						.Join(
							state.Users.Where(user => !user.IsDeleted),
							relation => relation.UserGuid,
							user => user.Guid,
							(relation, user) => new UserGroupUsersInfo
							{
								Guid = user.Guid,
								FullName = user.FullName,
								AccessType = relation.AccessType,
							})
						.ToArray(),
					AccessRights = state.AccessRights
						.Where(rule => !rule.IsGlobal && rule.UserGroupGuid == userGroup.Guid)
						.Select(rule => new AccessRightsForOneInfo
						{
							Id = rule.Id,
							IsGlobal = rule.IsGlobal,
							AccessType = rule.AccessType,
							Source = !state.SourcesById.TryGetValue(rule.SourceId ?? Identifiers.UnsetSource, out var source) ? null : new SourceSimpleInfo
							{
								Id = source.Id,
								Name = source.Name,
							},
							Block = !state.BlocksById.TryGetValue(rule.BlockId ?? 0, out var block) ? null : new BlockSimpleInfo
							{
								Id = block.Id,
								Guid = block.GlobalId,
								Name = block.Name,
							},
							Tag = !state.TagsById.TryGetValue(rule.TagId ?? 0, out var tag) ? null : new TagSimpleInfo
							{
								Id = tag.Id,
								Guid = tag.GlobalGuid,
								Name = tag.Name,
								Type = tag.Type,
								Resolution = tag.Resolution,
								SourceType = !state.SourcesById.TryGetValue(tag.SourceId, out var tagSource) ? SourceType.NotSet : tagSource.Type,
							},
						})
						.Where(x => x.Tag != null || x.Block != null || x.Source != null)
						.ToArray(),
					Subgroups = activeUserGroups
						.Where(subGroup => subGroup.ParentGuid == userGroup.Guid)
						.Select(subGroup => new UserGroupSimpleInfo
						{
							Guid = subGroup.Guid,
							Name = subGroup.Name,
						}
					).ToArray(),
				});
	}
}
