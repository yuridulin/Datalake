using Datalake.Database.Constants;
using Datalake.Database.Extensions;
using Datalake.Database.Functions;
using Datalake.Database.InMemory.Models;
using Datalake.Database.InMemory.Queries;
using Datalake.Database.Tables;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.UserGroups;
using LinqToDB;
using LinqToDB.Data;
using System.Collections.Immutable;

namespace Datalake.Database.InMemory.Repositories;

/// <summary>
/// Репозиторий работы с группами пользователей в памяти приложения
/// </summary>
public class UserGroupsMemoryRepository(DatalakeDataStore dataStore)
{
	#region API

	/// <summary>
	/// Создание новой группы пользователей
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="request">Параметры новой группы</param>
	/// <returns>Идентификатор созданной группы</returns>
	public async Task<UserGroupInfo> CreateAsync(
		DatalakeContext db, UserAuthInfo user, UserGroupCreateRequest request)
	{
		if (request.ParentGuid.HasValue)
		{
			user.ThrowIfNoAccessToUserGroup(AccessType.Manager, request.ParentGuid.Value);
		}
		else
		{
			user.ThrowIfNoGlobalAccess(AccessType.Manager);
		}

		return await ProtectedCreateAsync(db, user.Guid, request);
	}

	/// <summary>
	/// Получение информации о группе пользователей
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="guid">Идентификатор группы</param>
	/// <returns>Информация о группе</returns>
	public UserGroupInfo Read(UserAuthInfo user, Guid guid)
	{
		var rule = user.GetAccessToUserGroup(guid);
		if (!rule.HasAccess(AccessType.Viewer))
			throw Errors.NoAccess;

		var group = dataStore.State.UserGroupsInfo()
			.FirstOrDefault(x => x.Guid == guid)
			?? throw new NotFoundException($"группа {guid}");

		group.AccessRule = rule;

		return group;
	}

	/// <summary>
	/// Получение информации о группах пользователей
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <returns>Список групп</returns>
	public UserGroupInfo[] ReadAll(UserAuthInfo user)
	{
		var groups = dataStore.State.UserGroupsInfo();

		List<UserGroupInfo> groupsWithAccess = [];
		foreach (var group in groups)
		{
			group.AccessRule = user.GetAccessToUserGroup(group.Guid);
			if (group.AccessRule.HasAccess(AccessType.Viewer))
				groupsWithAccess.Add(group);
		}

		return groupsWithAccess.ToArray();
	}

	/// <summary>
	/// Получение информации о группе пользователей в иерархической структуре
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <returns>Дерево групп</returns>
	public UserGroupTreeInfo[] ReadAllAsTree(UserAuthInfo user)
	{
		var groups = ReadAll(user);

		return ReadChildren(null);

		UserGroupTreeInfo[] ReadChildren(Guid? guid)
		{
			return groups
				.Where(x => x.ParentGroupGuid == guid)
				.Select(x =>
				{
					var group = new UserGroupTreeInfo
					{
						Guid = x.Guid,
						Name = x.Name,
						ParentGuid = x.ParentGroupGuid,
						Description = x.Description,
						AccessRule = x.AccessRule,
						GlobalAccessType = x.GlobalAccessType,
						ParentGroupGuid = x.ParentGroupGuid,
						Children = ReadChildren(x.Guid),
					};

					if (!x.AccessRule.HasAccess(AccessType.Viewer))
					{
						group.Name = string.Empty;
						group.Description = string.Empty;
					}

					return group;
				})
				.Where(x => x.Children.Length > 0 || x.AccessRule.HasAccess(AccessType.Viewer))
				.ToArray();
		}
	}

	/// <summary>
	/// Получение информации о группе пользователей, включая пользователей, подгруппы и правила
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="guid">Идентификатор группы</param>
	/// <returns>Детальная информация о группе</returns>
	public UserGroupDetailedInfo ReadWithDetails(UserAuthInfo user, Guid guid)
	{
		var rule = user.GetAccessToUserGroup(guid);
		if (!rule.HasAccess(AccessType.Viewer))
			throw Errors.NoAccess;

		var group = dataStore.State.UserGroupsInfoWithDetails()
			.FirstOrDefault(x => x.Guid == guid)
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
	public async Task<bool> UpdateAsync(
		DatalakeContext db, UserAuthInfo user, Guid groupGuid, UserGroupUpdateRequest request)
	{
		user.ThrowIfNoAccessToUserGroup(AccessType.Editor, groupGuid);

		return await ProtectedUpdateAsync(db, user.Guid, groupGuid, request);
	}

	/// <summary>
	/// Изменение положения группы пользователей в иерархии
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <param name="parentGuid">Идентификатор вышестоящей группы</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> MoveAsync(
		DatalakeContext db, UserAuthInfo user, Guid groupGuid, Guid? parentGuid)
	{
		user.ThrowIfNoAccessToUserGroup(AccessType.Manager, groupGuid);

		if (parentGuid.HasValue)
		{
			user.ThrowIfNoAccessToUserGroup(AccessType.Manager, parentGuid.Value);
		}
		else
		{
			user.ThrowIfNoGlobalAccess(AccessType.Manager);
		}

		return await ProtectedMoveAsync(db, user.Guid, groupGuid, parentGuid);
	}

	/// <summary>
	/// Удаление группы пользователей
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> DeleteAsync(
		DatalakeContext db, UserAuthInfo user, Guid groupGuid)
	{
		user.ThrowIfNoAccessToUserGroup(AccessType.Manager, groupGuid);

		return await ProtectedDeleteAsync(db, user.Guid, groupGuid);
	}

	#endregion

	#region Действия

	internal async Task<UserGroupInfo> ProtectedCreateAsync(
		DatalakeContext db, Guid userGuid, UserGroupCreateRequest request)
	{
		// Проверки, не требующие стейта
		UserGroup newUserGroup = new()
		{
			Guid = Guid.NewGuid(),
			ParentGuid = request.ParentGuid,
			Name = request.Name,
			Description = request.Description,
		};

		AccessRights directRuleForUserGroup = new()
		{
			UserGroupGuid = newUserGroup.Guid,
			IsGlobal = true,
			AccessType = AccessType.NotSet,
		};

		// Блокируем стейт до завершения обновления
		DatalakeDataState currentState;
		using (await dataStore.AcquireWriteLockAsync())
		{
			currentState = dataStore.State;

			// Проверки на актуальном стейте
			if (currentState.UserGroups.Any(x => !x.IsDeleted && x.ParentGuid == request.ParentGuid && x.Name == request.Name))
				throw new AlreadyExistException(message: "группа " + request.Name);

			// Обновление в БД
			using var transaction = await db.BeginTransactionAsync();

			try
			{
				var group = await db.UserGroups
					.Value(x => x.Guid, newUserGroup.Guid)
					.Value(x => x.Name, newUserGroup.Name)
					.Value(x => x.ParentGuid, newUserGroup.ParentGuid)
					.Value(x => x.Description, newUserGroup.Description)
					.InsertWithOutputAsync();

				await db.AccessRights
					.Value(x => x.UserGroupGuid, directRuleForUserGroup.UserGroupGuid)
					.Value(x => x.IsGlobal, directRuleForUserGroup.IsGlobal)
					.Value(x => x.AccessType, directRuleForUserGroup.AccessType)
					.InsertAsync();

				await LogAsync(db, userGuid, group.Guid, $"Создана группа пользователей \"{group.Name}\"");

				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception("Не удалось создать группу пользователей в БД", ex);
			}

			// Обновление стейта в случае успешного обновления БД
			dataStore.UpdateStateWithinLock(state => state with
			{
				AccessRights = state.AccessRights.Add(directRuleForUserGroup),
				UserGroups = state.UserGroups.Add(newUserGroup),
			});
		}

		// Возвращение ответа
		var info = new UserGroupInfo
		{
			Guid = newUserGroup.Guid,
			Name = newUserGroup.Name,
			Description = newUserGroup.Description,
			ParentGroupGuid = newUserGroup.ParentGuid,
			GlobalAccessType = directRuleForUserGroup.AccessType,
		};

		return info;
	}

	internal async Task<bool> ProtectedUpdateAsync(
		DatalakeContext db, Guid userGuid, Guid groupGuid, UserGroupUpdateRequest request)
	{
		// Проверки, не требующие стейта
		UserGroup updatedUserGroup;
		AccessRights? updatedAccessRights = null;

		// Блокируем стейт до завершения обновления
		DatalakeDataState currentState;
		using (await dataStore.AcquireWriteLockAsync())
		{
			currentState = dataStore.State;

			// Проверки на актуальном стейте
			if (!currentState.UserGroupsByGuid.TryGetValue(groupGuid, out var userGroup))
				throw new NotFoundException(message: "группа " + groupGuid);

			if (currentState.UserGroups.Any(x =>
				!x.IsDeleted &&
				x.Guid != groupGuid &&
				x.ParentGuid == userGroup.ParentGuid
				&& x.Name == request.Name))
				throw new AlreadyExistException(message: "группа с таким же именем");

			updatedUserGroup = userGroup with
			{
				Name = request.Name,
				Description = request.Description,
			};

			var accessRights = currentState.AccessRights.FirstOrDefault(x => x.UserGroupGuid == groupGuid && x.IsGlobal);
			if (accessRights != null && accessRights.AccessType != request.AccessType)
				updatedAccessRights = accessRights with
				{
					AccessType = request.AccessType,
				};

			var newUsersRelations = request.Users
				.Select(u => new UserGroupRelation
				{
					UserGuid = u.Guid,
					UserGroupGuid = groupGuid,
					AccessType = u.AccessType,
				})
				.ToArray();

			// Обновление в БД
			using var transaction = await db.BeginTransactionAsync();

			try
			{
				await db.UserGroups
					.Where(x => x.Guid == groupGuid)
					.Set(x => x.Name, updatedUserGroup.Name)
					.Set(x => x.Description, updatedUserGroup.Description)
					.UpdateAsync();

				if (updatedAccessRights != null)
				{
					await db.AccessRights
						.Where(x => x.Id == updatedAccessRights.Id)
						.Set(x => x.AccessType, updatedAccessRights.AccessType)
						.UpdateAsync();
				}

				await db.UserGroupRelations
					.Where(x => x.UserGroupGuid == groupGuid)
					.DeleteAsync();

				await db.UserGroupRelations
					.BulkCopyAsync(newUsersRelations);

				await LogAsync(db, userGuid, groupGuid, $"Изменена группа пользователей: {userGroup.Name}", ObjectExtension.Difference(
					new { userGroup.Name, userGroup.Description, userGroup.ParentGuid, },
					new { request.Name, request.Description, request.ParentGuid, }));

				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception("Не удалось обновить группу пользователей в БД", ex);
			}

			// Обновление стейта в случае успешного обновления БД
			dataStore.UpdateStateWithinLock(state => state with
			{
				UserGroups = state.UserGroups.Replace(userGroup, updatedUserGroup),
				AccessRights = accessRights != null && updatedAccessRights != null
					? state.AccessRights.Replace(accessRights, updatedAccessRights)
					: state.AccessRights,
				UserGroupRelations = state.UserGroupRelations.RemoveAll(x => x.UserGroupGuid != groupGuid).AddRange(newUsersRelations),
			});
		}

		// Возвращение ответа
		return true;
	}

	internal async Task<bool> ProtectedMoveAsync(
		DatalakeContext db, Guid userGuid, Guid guid, Guid? parentGuid)
	{
		// Проверки, не требующие стейта
		UserGroup movedUserGroup;

		// Блокируем стейт до завершения обновления
		DatalakeDataState currentState;
		using (await dataStore.AcquireWriteLockAsync())
		{
			currentState = dataStore.State;

			// Проверки на актуальном стейте
			if (!currentState.UserGroupsByGuid.TryGetValue(guid, out var movingUserGroup))
				throw new NotFoundException(message: "группа " + guid);

			if (parentGuid.HasValue && parentGuid.Value == guid)
				throw new InvalidValueException("Группа не может быть родителем самой себе");

			if (parentGuid.HasValue && !currentState.UserGroupsByGuid.ContainsKey(parentGuid.Value))
				throw new NotFoundException($"Родительская группа {parentGuid.Value} не найдена");

			movedUserGroup = movingUserGroup with
			{
				ParentGuid = parentGuid,
			};

			// Обновление в БД
			using var transaction = await db.BeginTransactionAsync();

			try
			{
				await db.UserGroups
					.Where(x => x.Guid == guid)
					.Set(x => x.ParentGuid, parentGuid)
					.UpdateAsync();

				await LogAsync(db, userGuid, guid, $"Изменено расположение группы пользователей: {movingUserGroup.Name}", ObjectExtension.Difference(
					new { movingUserGroup.ParentGuid },
					new { movedUserGroup.ParentGuid }));

				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception("Не удалось переместить группу пользователей в БД", ex);
			}

			// Обновление стейта в случае успешного обновления БД
			dataStore.UpdateStateWithinLock(state => state with
			{
				UserGroups = state.UserGroups.Replace(movingUserGroup, movedUserGroup),
			});
		}

		// Возвращение ответа
		return true;
	}

	internal async Task<bool> ProtectedDeleteAsync(
		DatalakeContext db, Guid userGuid, Guid groupGuid)
	{
		// Проверки, не требующие стейта
		UserGroup updatedUserGroup;

		// Блокируем стейт до завершения обновления
		DatalakeDataState currentState;
		using (await dataStore.AcquireWriteLockAsync())
		{
			currentState = dataStore.State;

			// Проверки на актуальном стейте
			if (!currentState.UserGroupsByGuid.TryGetValue(groupGuid, out var userGroup))
				throw new NotFoundException(message: "группа " + groupGuid);

			updatedUserGroup = userGroup with { IsDeleted = true };

			// Обновление в БД
			using var transaction = await db.BeginTransactionAsync();

			try
			{
				await db.UserGroups
					.Where(x => x.Guid == groupGuid)
					.Set(x => x.IsDeleted, true)
					.UpdateAsync();

				await LogAsync(db, userGuid, groupGuid, $"Удалена группа пользователей: {userGroup.Name}");

				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception("Не удалось удалить группу пользователей из БД", ex);
			}

			// Обновление стейта в случае успешного обновления БД
			dataStore.UpdateStateWithinLock(state => state with
			{
				UserGroups = state.UserGroups.Replace(userGroup, updatedUserGroup),
			});
		}

		// Возвращение ответа
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
}
