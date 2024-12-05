using Datalake.Database.Constants;
using Datalake.Database.Enums;
using Datalake.Database.Exceptions;
using Datalake.Database.Extensions;
using Datalake.Database.Models.AccessRights;
using Datalake.Database.Models.Auth;
using Datalake.Database.Models.Blocks;
using Datalake.Database.Models.Sources;
using Datalake.Database.Models.Tags;
using Datalake.Database.Models.UserGroups;
using Datalake.Database.Tables;
using LinqToDB;
using LinqToDB.Data;

namespace Datalake.Database.Repositories;

/// <summary>
/// Репозиторий для работы с группами пользователей
/// </summary>
public class UserGroupsRepository(DatalakeContext db)
{
	#region Действия

	/// <summary>
	/// Создание новой группы пользователей
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="request">Параметры новой группы</param>
	/// <returns>Идентификатор созданной группы</returns>
	public async Task<Guid> CreateAsync(UserAuthInfo user, UserGroupCreateRequest request)
	{
		if (request.ParentGuid.HasValue)
		{
			AccessRepository.ThrowIfNoAccessToUserGroup(user, AccessType.Admin, request.ParentGuid.Value);
		}
		else
		{
			AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Admin);
		}
		User = user.Guid;

		return await CreateAsync(request);
	}

	/// <summary>
	/// Получение информации о группе пользователей
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="guid">Идентификатор группы</param>
	/// <returns>Информация о группе</returns>
	public async Task<UserGroupInfo> ReadAsync(UserAuthInfo user, Guid guid)
	{
		var rule = AccessRepository.GetAccessToUserGroup(user, guid);
		if (!rule.AccessType.HasAccess(AccessType.Viewer))
			throw Errors.NoAccess;

		var group = await db.UserGroupsRepository.GetInfo()
			.Where(x => x.Guid == guid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"группа {guid}");

		group.AccessRule = rule;

		return group;
	}

	/// <summary>
	/// Получение информации о группах пользователей
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <returns>Список групп</returns>
	public async Task<UserGroupInfo[]> ReadAllAsync(UserAuthInfo user)
	{
		var groups = await db.UserGroupsRepository.GetInfo()
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
	/// <param name="user">Информация о пользователе</param>
	/// <returns>Дерево групп</returns>
	public async Task<UserGroupTreeInfo[]> ReadAllAsTreeAsync(UserAuthInfo user)
	{
		var groups = await db.UserGroups
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
	/// <param name="user">Информация о пользователе</param>
	/// <param name="guid">Идентификатор группы</param>
	/// <returns>Детальная информация о группе</returns>
	public async Task<UserGroupDetailedInfo> ReadWithDetailsAsync(UserAuthInfo user, Guid guid)
	{
		var rule = AccessRepository.GetAccessToUserGroup(user, guid);
		if (!rule.AccessType.HasAccess(AccessType.Viewer))
			throw Errors.NoAccess;

		var group = await db.UserGroupsRepository.GetWithDetails()
			.Where(x => x.Guid == guid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: $"группа пользователей \"{guid}\"");

		group.AccessRule = rule;

		return group;
	}

	/// <summary>
	/// Изменение параметров группы пользователей
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <param name="request">Новые параметры группы</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> UpdateAsync(UserAuthInfo user, Guid groupGuid, UserGroupUpdateRequest request)
	{
		AccessRepository.ThrowIfNoAccessToUserGroup(user, AccessType.Editor, groupGuid);
		User = user.Guid;

		return await UpdateAsync(groupGuid, request);
	}

	/// <summary>
	/// Изменение положения группы пользователей в иерархии
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <param name="parentGuid">Идентификатор вышестоящей группы</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> MoveAsync(UserAuthInfo user, Guid groupGuid, Guid? parentGuid)
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
		User = user.Guid;

		return await MoveAsync(groupGuid, parentGuid);
	}

	/// <summary>
	/// Удаление группы пользователей
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> DeleteAsync(UserAuthInfo user, Guid groupGuid)
	{
		AccessRepository.ThrowIfNoAccessToUserGroup(user, AccessType.Admin, groupGuid);
		User = user.Guid;

		return await DeleteAsync(groupGuid);
	}

	#endregion

	#region Реализация

	Guid? User { get; set; } = null;

	internal async Task<Guid> CreateAsync(UserGroupCreateRequest request)
	{
		if (await db.UserGroups.AnyAsync(x => x.Name == request.Name && x.ParentGuid == request.ParentGuid))
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

		await LogAsync(group.Guid, $"Создана группа пользователей \"{group.Name}\"");

		await transaction.CommitAsync();

		AccessRepository.Update();

		return group.Guid;
	}

	internal async Task<bool> UpdateAsync(Guid groupGuid, UserGroupUpdateRequest request)
	{
		var group = await GetWithDetails()
			.Where(x => x.Guid == groupGuid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "группа " + groupGuid);

		if (await db.UserGroups.AnyAsync(x => x.Name == request.Name
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

		await LogAsync(groupGuid, $"Изменена группа пользователей: {group.Name}", ObjectExtension.Difference(
			new { group.Name, group.Description, ParentGuid = group.ParentGroupGuid, Users = group.Users.Select(u => new { u.Guid, u.AccessType }) },
			new { request.Name, request.Description, request.ParentGuid, Users = request.Users.Select(u => new { u.Guid, u.AccessType }) }));

		await transaction.CommitAsync();

		AccessRepository.Update();

		return true;
	}

	internal async Task<bool> MoveAsync(Guid guid, Guid? parentGuid)
	{
		using var transaction = await db.BeginTransactionAsync();

		var group = await db.UserGroups
			.Where(x => x.Guid == guid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "группа " + guid);

		await db.UserGroups
			.Where(x => x.Guid == guid)
			.Set(x => x.ParentGuid, parentGuid)
			.UpdateAsync();

		await LogAsync(guid, $"Изменено расположение группы пользователей: {group.Name}", ObjectExtension.Difference(
			new { group.ParentGuid },
			new { ParentGuid = parentGuid }));

		await transaction.CommitAsync();

		AccessRepository.Update();

		return true;
	}

	internal async Task<bool> DeleteAsync(Guid groupGuid)
	{
		using var transaction = await db.BeginTransactionAsync();

		var group = await db.UserGroups
			.FirstOrDefaultAsync(x => x.Guid == groupGuid)
			?? throw new NotFoundException(message: "группа " + groupGuid);

		await db.AccessRights
			.Where(x => x.UserGroupGuid == groupGuid)
			.DeleteAsync();

		await db.UserGroupRelations
			.Where(x => x.UserGroupGuid == groupGuid)
			.DeleteAsync();

		await db.UserGroups
			.Where(x => x.Guid == groupGuid)
			.DeleteAsync();

		await LogAsync(groupGuid, $"Удалена группа пользователей: {group.Name}");

		await transaction.CommitAsync();

		AccessRepository.Update();

		return true;
	}

	private async Task LogAsync(Guid guid, string message, string? details = null)
	{
		await db.InsertAsync(new Log
		{
			Category = LogCategory.UserGroups,
			RefId = guid.ToString(),
			Text = message,
			Type = LogType.Success,
			UserGuid = User,
			Details = details,
		});
	}

	#endregion

	#region Запросы

	internal IQueryable<UserGroupInfo> GetInfo()
	{
		return db.UserGroups
			.Select(x => new UserGroupInfo
			{
				Guid = x.Guid,
				Name = x.Name,
				Description = x.Description,
				ParentGroupGuid = x.ParentGuid,
			});
	}

	internal IQueryable<UserGroupDetailedInfo> GetWithDetails()
	{
		var query =
			from usergroup in db.UserGroups
			from globalAccess in db.AccessRights.InnerJoin(x => x.UserGroupGuid == usergroup.Guid && x.IsGlobal)
			select new UserGroupDetailedInfo
			{
				Guid = usergroup.Guid,
				Name = usergroup.Name,
				Description = usergroup.Description,
				ParentGroupGuid = usergroup.ParentGuid,
				GlobalAccessType = globalAccess.AccessType,
				Users =
					from rel in db.UserGroupRelations.LeftJoin(x => x.UserGroupGuid == usergroup.Guid)
					from u in db.Users.InnerJoin(x => x.Guid == rel.UserGuid)
					select new UserGroupUsersInfo
					{
						Guid = u.Guid,
						FullName = u.FullName,
						AccessType = rel.AccessType,
					},
				AccessRights =
					from rights in db.AccessRights.InnerJoin(x => x.UserGroupGuid == usergroup.Guid)
					from source in db.Sources.LeftJoin(x => x.Id == rights.SourceId)
					from block in db.Blocks.LeftJoin(x => x.Id == rights.BlockId)
					from tag in db.Tags.LeftJoin(x => x.Id == rights.TagId)
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
						},
					},
				Subgroups =
					from subgroup in db.UserGroups.LeftJoin(x => x.ParentGuid == usergroup.Guid)
					select new UserGroupSimpleInfo
					{
						Guid = subgroup.Guid,
						Name = subgroup.Name,
					},
			};

		return query;
	}

	#endregion
}
