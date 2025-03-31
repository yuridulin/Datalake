using Datalake.Database.Constants;
using Datalake.Database.Extensions;
using Datalake.Database.Tables;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.AccessRights;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Blocks;
using Datalake.PublicApi.Models.Sources;
using Datalake.PublicApi.Models.Tags;
using Datalake.PublicApi.Models.UserGroups;
using LinqToDB;
using LinqToDB.Data;

namespace Datalake.Database.Repositories;

/// <summary>
/// Репозиторий для работы с группами пользователей
/// </summary>
public static class UserGroupsRepository
{
	#region Действия

	/// <summary>
	/// Создание новой группы пользователей
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="request">Параметры новой группы</param>
	/// <returns>Идентификатор созданной группы</returns>
	public static async Task<Guid> CreateAsync(
		DatalakeContext db, UserAuthInfo user, UserGroupCreateRequest request)
	{
		if (request.ParentGuid.HasValue)
		{
			AccessRepository.ThrowIfNoAccessToUserGroup(user, AccessType.Admin, request.ParentGuid.Value);
		}
		else
		{
			AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Admin);
		}

		return await CreateAsync(db, user.Guid, request);
	}

	/// <summary>
	/// Получение информации о группе пользователей
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="guid">Идентификатор группы</param>
	/// <returns>Информация о группе</returns>
	public static async Task<UserGroupInfo> ReadAsync(
		DatalakeContext db, UserAuthInfo user, Guid guid)
	{
		var rule = AccessRepository.GetAccessToUserGroup(user, guid);
		if (!rule.AccessType.HasAccess(AccessType.Viewer))
			throw Errors.NoAccess;

		var group = await GetInfo(db)
			.Where(x => x.Guid == guid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"группа {guid}");

		group.AccessRule = rule;

		return group;
	}

	/// <summary>
	/// Получение информации о группах пользователей
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <returns>Список групп</returns>
	public static async Task<UserGroupInfo[]> ReadAllAsync(
		DatalakeContext db, UserAuthInfo user)
	{
		var groups = await GetInfo(db)
			.ToArrayAsync();

		foreach (var group in groups)
		{
			group.AccessRule = AccessRepository.GetAccessToUserGroup(user, group.Guid);
		}

		return groups
			.Where(x => x.AccessRule.AccessType.HasAccess(AccessType.Viewer))
			.ToArray();
	}

	/// <summary>
	/// Получение информации о группе пользователей в иерархической структуре
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <returns>Дерево групп</returns>
	public static async Task<UserGroupTreeInfo[]> ReadAllAsTreeAsync(
		DatalakeContext db, UserAuthInfo user)
	{
		var groups = await UserGroupsNotDeleted(db)
			.Select(x => new UserGroupTreeInfo
			{
				Guid = x.Guid,
				Name = x.Name,
				ParentGuid = x.ParentGuid,
				Description = x.Description,
			})
			.ToArrayAsync();

		foreach (var group in groups)
		{
			group.AccessRule = AccessRepository.GetAccessToUserGroup(user, group.Guid);
		}

		return ReadChildren(null);

		UserGroupTreeInfo[] ReadChildren(Guid? guid)
		{
			return groups
				.Where(x => x.ParentGuid == guid)
				.Select(x =>
				{
					var group = new UserGroupTreeInfo
					{
						Guid = x.Guid,
						Name = x.Name,
						ParentGuid = x.ParentGuid,
						Description = x.Description,
						AccessRule = x.AccessRule,
						ParentGroupGuid = x.ParentGroupGuid,
						Children = ReadChildren(x.Guid),
					};

					if (!x.AccessRule.AccessType.HasAccess(AccessType.Viewer))
					{
						group.Name = string.Empty;
						group.Description = string.Empty;
					}

					return group;
				})
				.Where(x => x.Children.Length > 0 || x.AccessRule.AccessType.HasAccess(AccessType.Viewer))
				.ToArray();
		}
	}

	/// <summary>
	/// Получение информации о группе пользователей, включая пользователей, подгруппы и правила
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="guid">Идентификатор группы</param>
	/// <returns>Детальная информация о группе</returns>
	public static async Task<UserGroupDetailedInfo> ReadWithDetailsAsync(
		DatalakeContext db, UserAuthInfo user, Guid guid)
	{
		var rule = AccessRepository.GetAccessToUserGroup(user, guid);
		if (!rule.AccessType.HasAccess(AccessType.Viewer))
			throw Errors.NoAccess;

		var group = await GetWithDetails(db)
			.Where(x => x.Guid == guid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: $"группа пользователей \"{guid}\"");

		group.AccessRule = rule;

		return group;
	}

	/// <summary>
	/// Изменение параметров группы пользователей
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <param name="request">Новые параметры группы</param>
	/// <returns>Флаг успешного завершения</returns>
	public static async Task<bool> UpdateAsync(
		DatalakeContext db, UserAuthInfo user, Guid groupGuid, UserGroupUpdateRequest request)
	{
		AccessRepository.ThrowIfNoAccessToUserGroup(user, AccessType.Editor, groupGuid);

		return await UpdateAsync(db, user.Guid, groupGuid, request);
	}

	/// <summary>
	/// Изменение положения группы пользователей в иерархии
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <param name="parentGuid">Идентификатор вышестоящей группы</param>
	/// <returns>Флаг успешного завершения</returns>
	public static async Task<bool> MoveAsync(
		DatalakeContext db, UserAuthInfo user, Guid groupGuid, Guid? parentGuid)
	{
		AccessRepository.ThrowIfNoAccessToUserGroup(user, AccessType.Admin, groupGuid);

		if (parentGuid.HasValue)
		{
			AccessRepository.ThrowIfNoAccessToUserGroup(user, AccessType.Editor, parentGuid.Value);
		}
		else
		{
			AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Editor);
		}

		return await MoveAsync(db, user.Guid, groupGuid, parentGuid);
	}

	/// <summary>
	/// Удаление группы пользователей
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <returns>Флаг успешного завершения</returns>
	public static async Task<bool> DeleteAsync(
		DatalakeContext db, UserAuthInfo user, Guid groupGuid)
	{
		AccessRepository.ThrowIfNoAccessToUserGroup(user, AccessType.Admin, groupGuid);

		return await DeleteAsync(db, user.Guid, groupGuid);
	}

	#endregion

	#region Реализация

	internal static async Task<Guid> CreateAsync(
		DatalakeContext db, Guid userGuid, UserGroupCreateRequest request)
	{
		if (await UserGroupsNotDeleted(db).AnyAsync(x => x.Name == request.Name && x.ParentGuid == request.ParentGuid))
			throw new AlreadyExistException(message: "группа " + request.Name);

		using var transaction = await db.BeginTransactionAsync();

		var group = await db.UserGroups
			.Value(x => x.Guid, Guid.NewGuid())
			.Value(x => x.Name, request.Name)
			.Value(x => x.ParentGuid, request.ParentGuid)
			.Value(x => x.Description, request.Description)
			.InsertWithOutputAsync();

		await db.AccessRights
			.Value(x => x.UserGroupGuid, group.Guid)
			.Value(x => x.IsGlobal, true)
			.Value(x => x.AccessType, AccessType.Viewer)
			.InsertAsync();

		await LogAsync(db, userGuid, group.Guid, $"Создана группа пользователей \"{group.Name}\"");

		await transaction.CommitAsync();

		AccessRepository.Update();

		return group.Guid;
	}

	internal static async Task<bool> UpdateAsync(
		DatalakeContext db, Guid userGuid, Guid groupGuid, UserGroupUpdateRequest request)
	{
		var group = await GetWithDetails(db)
			.Where(x => x.Guid == groupGuid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "группа " + groupGuid);

		if (await UserGroupsNotDeleted(db).AnyAsync(x => x.Name == request.Name
			&& x.ParentGuid == request.ParentGuid
			&& x.Guid != groupGuid))
			throw new AlreadyExistException(message: "группа " + request.Name);

		using var transaction = await db.BeginTransactionAsync();

		await db.UserGroups
			.Where(x => x.Guid == groupGuid)
			.Set(x => x.Name, request.Name)
			.Set(x => x.Description, request.Description)
			.UpdateAsync();

		await db.AccessRights
			.Where(x => x.UserGroupGuid == groupGuid && x.IsGlobal)
			.Set(x => x.AccessType, request.AccessType)
			.UpdateAsync();

		await db.UserGroupRelations
			.Where(x => x.UserGroupGuid == groupGuid)
			.DeleteAsync();

		await db.UserGroupRelations
			.BulkCopyAsync(request.Users.Select(u => new UserGroupRelation
			{
				UserGuid = u.Guid,
				UserGroupGuid = groupGuid,
				AccessType = u.AccessType,
			}));

		await LogAsync(db, userGuid, groupGuid, $"Изменена группа пользователей: {group.Name}", ObjectExtension.Difference(
			new { group.Name, group.Description, ParentGuid = group.ParentGroupGuid, Users = group.Users.Select(u => new { u.Guid, u.AccessType }) },
			new { request.Name, request.Description, request.ParentGuid, Users = request.Users.Select(u => new { u.Guid, u.AccessType }) }));

		await transaction.CommitAsync();

		AccessRepository.Update();

		return true;
	}

	internal static async Task<bool> MoveAsync(
		DatalakeContext db, Guid userGuid, Guid guid, Guid? parentGuid)
	{
		using var transaction = await db.BeginTransactionAsync();

		var group = await UserGroupsNotDeleted(db)
			.Where(x => x.Guid == guid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "группа " + guid);

		await db.UserGroups
			.Where(x => x.Guid == guid)
			.Set(x => x.ParentGuid, parentGuid)
			.UpdateAsync();

		await LogAsync(db, userGuid, guid, $"Изменено расположение группы пользователей: {group.Name}", ObjectExtension.Difference(
			new { group.ParentGuid },
			new { ParentGuid = parentGuid }));

		await transaction.CommitAsync();

		AccessRepository.Update();

		return true;
	}

	internal static async Task<bool> DeleteAsync(
		DatalakeContext db, Guid userGuid, Guid groupGuid)
	{
		using var transaction = await db.BeginTransactionAsync();

		var group = await UserGroupsNotDeleted(db)
			.FirstOrDefaultAsync(x => x.Guid == groupGuid)
			?? throw new NotFoundException(message: "группа " + groupGuid);

		/*await db.AccessRights
			.Where(x => x.UserGroupGuid == groupGuid)
			.DeleteAsync();

		await db.UserGroupRelations
			.Where(x => x.UserGroupGuid == groupGuid)
			.DeleteAsync();*/

		await db.UserGroups
			.Where(x => x.Guid == groupGuid)
			.Set(x => x.IsDeleted, true)
			.UpdateAsync();

		await LogAsync(db, userGuid, groupGuid, $"Удалена группа пользователей: {group.Name}");

		await transaction.CommitAsync();

		AccessRepository.Update();

		return true;
	}

	private static async Task LogAsync(DatalakeContext db, Guid authorGuid, Guid guid, string message, string? details = null)
	{
		await db.InsertAsync(new Log
		{
			Category = LogCategory.UserGroups,
			RefId = guid.ToString(),
			AffectedUserGroupGuid = guid,
			Text = message,
			Type = LogType.Success,
			AuthorGuid = authorGuid,
			Details = details,
		});
	}

	#endregion

	#region Запросы

	internal static IQueryable<UserGroup> UserGroupsNotDeleted(DatalakeContext db)
	{
		return db.UserGroups
			.Where(x => !x.IsDeleted);
	}

	internal static IQueryable<UserGroupInfo> GetInfo(DatalakeContext db)
	{
		return db.UserGroups
			.Where(x => !x.IsDeleted)
			.Select(x => new UserGroupInfo
			{
				Guid = x.Guid,
				Name = x.Name,
				Description = x.Description,
				ParentGroupGuid = x.ParentGuid,
			});
	}

	internal static IQueryable<UserGroupDetailedInfo> GetWithDetails(DatalakeContext db)
	{
		var query =
			from usergroup in db.UserGroups.Where(x => !x.IsDeleted)
			from globalAccess in db.AccessRights.InnerJoin(x => x.UserGroupGuid == usergroup.Guid && x.IsGlobal)
			select new UserGroupDetailedInfo
			{
				Guid = usergroup.Guid,
				Name = usergroup.Name,
				Description = usergroup.Description,
				ParentGroupGuid = usergroup.ParentGuid,
				GlobalAccessType = globalAccess.AccessType,
				Users = (
					from rel in db.UserGroupRelations.LeftJoin(x => x.UserGroupGuid == usergroup.Guid)
					from u in db.Users.InnerJoin(x => x.Guid == rel.UserGuid && !x.IsDeleted)
					select new UserGroupUsersInfo
					{
						Guid = u.Guid,
						FullName = u.FullName,
						AccessType = rel.AccessType,
					}
				).ToArray(),
				AccessRights = (
					from rights in db.AccessRights.InnerJoin(x => x.UserGroupGuid == usergroup.Guid)
					from source in db.Sources.LeftJoin(x => x.Id == rights.SourceId && !x.IsDeleted)
					from block in db.Blocks.LeftJoin(x => x.Id == rights.BlockId && !x.IsDeleted)
					from tag in db.Tags.LeftJoin(x => x.Id == rights.TagId && !x.IsDeleted)
					from tag_source in db.Sources.LeftJoin(x => x.Id == tag.SourceId && !x.IsDeleted)
					select new AccessRightsForOneInfo
					{
						Id = rights.Id,
						IsGlobal = rights.IsGlobal,
						AccessType = rights.AccessType,
						Source = source == null ? null : new SourceSimpleInfo
						{
							Id = source.Id,
							Name = source.Name,
						},
						Block = block == null ? null : new BlockSimpleInfo
						{
							Id = block.Id,
							Guid = block.GlobalId,
							Name = block.Name,
						},
						Tag = tag == null ? null : new TagSimpleInfo
						{
							Id = tag.Id,
							Guid = tag.GlobalGuid,
							Name = tag.Name,
							Type = tag.Type,
							Frequency = tag.Frequency,
							SourceType = tag_source == null ? SourceType.NotSet : tag_source.Type,
						},
					}
				)
				.Where(x => x.Tag != null || x.Block != null || x.Source != null)
				.ToArray(),
				Subgroups = (
					from subgroup in db.UserGroups.LeftJoin(x => x.ParentGuid == usergroup.Guid && !x.IsDeleted)
					select new UserGroupSimpleInfo
					{
						Guid = subgroup.Guid,
						Name = subgroup.Name,
					}
				).ToArray(),
			};

		return query;
	}

	#endregion
}
