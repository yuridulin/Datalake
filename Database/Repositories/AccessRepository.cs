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
using Datalake.Database.Models.Users;
using Datalake.Database.Tables;
using LinqToDB;
using LinqToDB.Data;
using System.Collections.Concurrent;

namespace Datalake.Database.Repositories;

/// <summary>
/// Репозиторий для работы с правами доступа
/// </summary>
public class AccessRepository(DatalakeContext db)
{
	#region Действия

	/// <summary>
	/// Получение информации о пользователе по данным из EnergoId
	/// </summary>
	/// <param name="info">Данные о учетной записи</param>
	/// <returns>Информация о пользователе, включая доступ</returns>
	public async Task<UserAuthInfo> AuthenticateAsync(UserEnergoIdInfo info)
	{
		var user = await db.Users
			.Where(x => x.Type == UserType.EnergoId)
			.Where(x => x.EnergoIdGuid != null && x.EnergoIdGuid == info.EnergoIdGuid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "указанная учётная запись по guid");

		return GetAuthInfo(user);
	}

	/// <summary>
	/// Получение информации о пользователе по логину и паролю
	/// </summary>
	/// <param name="loginPass">Логин и пароль</param>
	/// <returns>Информация о пользователе, включая доступ</returns>
	public async Task<UserAuthInfo> AuthenticateAsync(UserLoginPass loginPass)
	{
		var user = await db.Users
			.Where(x => x.Type == UserType.Local)
			.Where(x => x.Login != null && (x.Login.ToLower().Trim() == loginPass.Login.ToLower().Trim()))
			.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "указанная учётная запись по логину");

		if (string.IsNullOrEmpty(user.PasswordHash))
		{
			throw new InvalidValueException(message: "пароль не задан");
		}

		if (!user.PasswordHash.Equals(UsersRepository.GetHashFromPassword(loginPass.Password)))
		{
			throw new ForbiddenException(message: "пароль не подходит");
		}

		return GetAuthInfo(user);
	}

	/// <summary>
	/// Получение списка правил доступа для запрошенных объектов
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <param name="userGroupGuid">Идентификатор группы пользователей</param>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="tagId">Идентификатор тега</param>
	/// <returns>Список правил доступа</returns>
	public async Task<AccessRightsInfo[]> GetRightsAsync(
		UserAuthInfo user,
		Guid? userGuid,
		Guid? userGroupGuid,
		int? sourceId,
		int? blockId,
		int? tagId)
	{
		ThrowIfNoGlobalAccess(user, AccessType.User);

		var rights = await QueryRights(userGuid, userGroupGuid, sourceId, blockId, tagId).ToArrayAsync();

		return rights;
	}

	/// <summary>
	/// Получение списка статичный учетных записей вместе с информацией о доступе
	/// </summary>
	/// <returns>Список статичных учетных записей</returns>
	public async Task<UserStaticAuthInfo[]> GetStaticUsersAsSystemAsync()
	{
		var staticUsers = await db.Users
			.Where(x => x.Type == UserType.Static)
			.ToArrayAsync();

		return staticUsers
			.Select(x => new UserStaticAuthInfo { Guid = x.Guid, Host = x.StaticHost, AuthInfo = GetAuthInfo(x), })
			.ToArray();
	}

	/// <summary>
	/// Применение изменений прав доступа
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="request">Новые права доступа</param>
	public async Task ApplyChangesAsync(UserAuthInfo user, AccessRightsApplyRequest request)
	{
		ThrowIfNoGlobalAccess(user, AccessType.Admin);

		if (request.UserGroupGuid.HasValue)
		{
			await SetUserGroupRightsAsync(request.UserGroupGuid.Value, request.Rights);
		}
		else if (request.UserGuid.HasValue)
		{
			await SetUserRightsAsync(request.UserGuid.Value, request.Rights);
		}
		else if (request.SourceId.HasValue)
		{
			await SetSourceRightsAsync(request.SourceId.Value, request.Rights);
		}
		else if (request.BlockId.HasValue)
		{
			await SetBlockRightsAsync(request.BlockId.Value, request.Rights);
		}
		else if (request.TagId.HasValue)
		{
			await SetTagRightsAsync(request.TagId.Value, request.Rights);
		}
	}

	#endregion

	#region Реализация

	internal async Task SetUserGroupRightsAsync(Guid userGroupGuid, AccessRightsIdInfo[] rights)
	{
		using var transaction = await db.BeginTransactionAsync();

		try
		{
			await db.AccessRights
				.Where(x => x.UserGroupGuid == userGroupGuid && !x.IsGlobal)
				.DeleteAsync();

			await db.AccessRights
				.BulkCopyAsync(rights.Select(x => new AccessRights
				{
					IsGlobal = false,
					UserGroupGuid = userGroupGuid,
					AccessType = x.AccessType,
					SourceId = x.SourceId,
					BlockId = x.BlockId,
					TagId = x.TagId,
				}));

			await transaction.CommitAsync();

			Update();
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			throw new DatabaseException(message: "не удалось обновить права доступа", ex);
		}
	}

	internal async Task SetUserRightsAsync(Guid userGuid, AccessRightsIdInfo[] rights)
	{
		using var transaction = await db.BeginTransactionAsync();

		try
		{
			await db.AccessRights
				.Where(x => x.UserGuid == userGuid && !x.IsGlobal)
				.DeleteAsync();

			await db.AccessRights
				.BulkCopyAsync(rights.Select(x => new AccessRights
				{
					IsGlobal = false,
					UserGuid = userGuid,
					AccessType = x.AccessType,
					SourceId = x.SourceId,
					BlockId = x.BlockId,
					TagId = x.TagId,
				}));

			await transaction.CommitAsync();

			Update();
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			throw new DatabaseException(message: "не удалось обновить права доступа", ex);
		}
	}

	internal async Task SetSourceRightsAsync(int sourceId, AccessRightsIdInfo[] rights)
	{
		using var transaction = await db.BeginTransactionAsync();

		try
		{
			await db.AccessRights
				.Where(x => x.SourceId == sourceId && !x.IsGlobal)
				.DeleteAsync();

			await db.AccessRights
				.BulkCopyAsync(rights.Select(x => new AccessRights
				{
					IsGlobal = false,
					AccessType = x.AccessType,
					SourceId = sourceId,
					UserGroupGuid = x.UserGroupGuid,
					UserGuid = x.UserGuid,
				}));

			await transaction.CommitAsync();

			Update();
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			throw new DatabaseException(message: "не удалось обновить права доступа", ex);
		}
	}

	internal async Task SetBlockRightsAsync(int blockId, AccessRightsIdInfo[] rights)
	{
		using var transaction = await db.BeginTransactionAsync();

		try
		{
			await db.AccessRights
				.Where(x => x.BlockId == blockId && !x.IsGlobal)
				.DeleteAsync();

			await db.AccessRights
				.BulkCopyAsync(rights.Select(x => new AccessRights
				{
					IsGlobal = false,
					AccessType = x.AccessType,
					BlockId = blockId,
					UserGroupGuid = x.UserGroupGuid,
					UserGuid = x.UserGuid,
				}));

			await transaction.CommitAsync();

			Update();
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			throw new DatabaseException(message: "не удалось обновить права доступа", ex);
		}
	}

	internal async Task SetTagRightsAsync(int tagId, AccessRightsIdInfo[] rights)
	{
		using var transaction = await db.BeginTransactionAsync();

		try
		{
			await db.AccessRights
				.Where(x => x.TagId == tagId && !x.IsGlobal)
				.DeleteAsync();

			await db.AccessRights
				.BulkCopyAsync(rights.Select(x => new AccessRights
				{
					IsGlobal = false,
					AccessType = x.AccessType,
					TagId = tagId,
					UserGroupGuid = x.UserGroupGuid,
					UserGuid = x.UserGuid,
				}));

			await transaction.CommitAsync();

			Update();
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			throw new DatabaseException(message: "не удалось обновить права доступа", ex);
		}
	}

	#endregion

	#region Кэш

	/// <summary>
	/// Время последнего обновления кэша учетных записей
	/// </summary>
	public static DateTime LastUpdate { get; set; } = DateTime.MinValue;

	/// <summary>
	/// Обновление времени последнего изменения кэша.
	/// По обновлению этого времени служба, изменяющая кэш, понимает, что нужно его изменить
	/// </summary>
	internal static void Update()
	{
		lock (locker)
		{
			LastUpdate = DateFormats.GetCurrentDateTime();
		}
	}

	/// <summary>
	/// Глобальный кэш учетных записей с вычисленным доступом ко всем объектам
	/// </summary>
	public static ConcurrentDictionary<Guid, UserAuthInfo> UserRights { get; set; } = [];

	/// <summary>
	/// Вычисление доступа ко всем объектам для каждой учетной записи и обновление кэша
	/// </summary>
	public async Task RebuildUserRightsCacheAsync()
	{
		var userGroupsDb = await db.UserGroups
				.Select(g => new
				{
					g.Guid,
					g.ParentGuid,
					g.Name,
				})
				.ToArrayAsync();

		var usersDb = await db.Users
			.Select(u => new
			{
				u.Guid,
				Name = u.FullName ?? u.Login ?? string.Empty,
			})
			.ToArrayAsync();

		var userRelationsDb = await db.UserGroupRelations
			.ToArrayAsync();

		var blocksDb = await db.Blocks
			.Select(x => new
			{
				x.Id,
				x.ParentId,
				x.Name,
			})
			.ToArrayAsync();

		var accessRightsDb = await db.AccessRights
			.ToArrayAsync();

		var sourcesDb = await db.Sources
			.Where(x => x.Id > 0)
			.Select(x => new
			{
				x.Id,
				x.Name,
			})
			.ToArrayAsync();

		var tagsDb = await db.Tags
			.Select(x => new
			{
				x.Id,
				x.Name,
				Guid = x.GlobalGuid,
			})
			.ToArrayAsync();

		var tagsRelationsDb = await db.BlockTags
			.ToArrayAsync();

		AccessRights defaultRule = new() { Id = 0, AccessType = AccessType.NotSet, };

		var userGroupsDict = userGroupsDb.ToDictionary(x => x.Guid);
		var userGroupsList = userGroupsDb
			.Select(group =>
			{
				var parents = new List<Guid> { group.Guid };
				Guid? parentKey = group.ParentGuid;

				while (parentKey.HasValue && userGroupsDict.TryGetValue(parentKey.Value, out var parent))
				{
					parents.Add(parentKey.Value);
					parentKey = parent.ParentGuid;
				}

				return new
				{
					group.Guid,
					group.Name,
					IdWithParents = parents.ToArray(),
				};
			})
			.ToArray();

		var blocksDict = blocksDb.ToDictionary(x => x.Id);
		var blocksList = blocksDb
			.Select(x =>
			{
				var parents = new List<int> { x.Id };
				int? parentKey = x.ParentId;

				while (parentKey.HasValue && blocksDict.TryGetValue(parentKey.Value, out var parent))
				{
					parents.Add(parentKey.Value);
					parentKey = parent.ParentId;
				}

				return new
				{
					x.Id,
					x.Name,
					IdWithParents = parents.ToArray(),
				};
			})
			.ToArray();

		var tagsList = tagsDb
			.Select(tagDb => new
			{
				tagDb.Id,
				tagDb.Name,
				tagDb.Guid,
				Blocks = tagsRelationsDb.Where(x => x.TagId == tagDb.Id).Select(x => x.BlockId).ToArray(),
			})
			.ToArray();

		var userGroupsRights = userGroupsList
			.Select(group =>
			{
				var drectAccessToBlock = accessRightsDb
					.Where(x => x.UserGroupGuid == group.Guid && x.BlockId.HasValue)
					.ToDictionary(x => x.BlockId!.Value);

				var directAccessToSource = accessRightsDb
					.Where(x => x.UserGroupGuid == group.Guid && x.SourceId.HasValue)
					.ToDictionary(x => x.SourceId!.Value);

				var directAccessToTag = accessRightsDb
					.Where(x => x.UserGroupGuid == group.Guid && x.TagId.HasValue)
					.ToDictionary(x => x.TagId!.Value);


				var globalRule = accessRightsDb.FirstOrDefault(x => x.UserGroupGuid == group.Guid && x.IsGlobal)
					?? defaultRule;

				var groupBlocks = blocksList
					.Select(block =>
					{
						var ruleSet = new List<AccessRights>() { globalRule };

						var directRule = block.IdWithParents
							.Select(id => drectAccessToBlock.TryGetValue(id, out var r) ? r : null)
							.FirstOrDefault(r => r != null);

						if (directRule != null)
							ruleSet.Add(directRule);

						var chosenRule = ruleSet.OrderByDescending(x => x.AccessType).First();

						return new
						{
							block.Id,
							block.Name,
							block.IdWithParents,
							Rule = chosenRule,
						};
					})
					.ToArray();

				var groupSources = sourcesDb
					.Select(source =>
					{
						var ruleSet = new List<AccessRights>() { globalRule };

						var directRule = directAccessToSource.TryGetValue(source.Id, out var r) ? r : null;
						if (directRule != null)
							ruleSet.Add(directRule);

						var chosenRule = ruleSet.OrderByDescending(x => x.AccessType).First();

						return new
						{
							source.Id,
							source.Name,
							Rule = chosenRule,
						};
					})
					.ToArray();

				var groupTags = tagsList
					.Select(tag =>
					{
						var ruleSet = new List<AccessRights>() { globalRule };

						var directRule = directAccessToTag.TryGetValue(tag.Id, out var r) ? r : null;
						if (directRule != null)
							ruleSet.Add(directRule);

						var rulesFromBlocks = groupBlocks
							.Where(b => tag.Blocks.Contains(b.Id))
							.Select(x => x.Rule)
							.ToArray();

						ruleSet.AddRange(rulesFromBlocks);

						var chosenRule = ruleSet.OrderByDescending(x => x.AccessType).First();

						return new
						{
							tag.Id,
							tag.Name,
							tag.Blocks,
							Rule = chosenRule,
						};
					})
					.ToArray();

				return new
				{
					group.Guid,
					group.IdWithParents,
					Sources = groupSources,
					Blocks = groupBlocks,
					Tags = groupTags,
				};
			})
			.ToArray();

		var userRights = usersDb
			.Select(user =>
			{
				var relationsToGroups = userRelationsDb
					.Where(x => x.UserGuid == user.Guid)
					.ToDictionary(x => x.UserGroupGuid);

				var directAccessToBlock = accessRightsDb
					.Where(x => x.UserGuid == user.Guid && x.BlockId.HasValue)
					.ToDictionary(x => x.BlockId!.Value);

				var directAccessToSource = accessRightsDb
					.Where(x => x.UserGuid == user.Guid && x.SourceId.HasValue)
					.ToDictionary(x => x.SourceId!.Value);

				var directAccessToTags = accessRightsDb
					.Where(x => x.UserGuid == user.Guid && x.TagId.HasValue)
					.ToDictionary(x => x.TagId!.Value);


				var globalRule = accessRightsDb.FirstOrDefault(x => x.UserGuid == user.Guid && x.IsGlobal)
					?? defaultRule;

				var userGroups = userGroupsRights
					.Select(x =>
					{
						var ruleSet = new List<UserGroupRelation>()
						{
								new() {
									AccessType = globalRule.AccessType,
									UserGroupGuid = x.Guid,
									UserGuid = user.Guid
								},
						};

						var directRule = x.IdWithParents
							.Select(id => relationsToGroups.TryGetValue(id, out var r) ? r : null)
							.FirstOrDefault(r => r != null);

						if (directRule != null)
							ruleSet.Add(directRule);

						var chosenRule = ruleSet.OrderByDescending(x => x.AccessType).First();

						return new
						{
							x.Guid,
							Rule = chosenRule,
							x.Sources,
							x.Blocks,
							x.Tags,
						};
					})
					.ToArray();

				var userBlocks = blocksList
					.Select(block =>
					{
						var ruleSet = new List<AccessRights>() { globalRule };

						var directRule = block.IdWithParents
							.Select(x => directAccessToBlock.TryGetValue(x, out var r) ? r : null)
							.FirstOrDefault(r => r != null);

						if (directRule != null)
							ruleSet.Add(directRule);

						// так как мы уже вычислили права доступа каждой группы на каждый блок,
						// нам не нужно подниматься по иерархии еще раз
						var rulesFromGroups = userGroups
							.SelectMany(x => x.Blocks.Where(b => b.Id == block.Id))
							.Select(x => x.Rule)
							.ToArray();

						ruleSet.AddRange(rulesFromGroups);

						var chosenRule = ruleSet.OrderByDescending(x => x.AccessType).First();

						return new
						{
							block.Id,
							Rule = chosenRule,
						};
					})
					.ToArray();

				var userSources = sourcesDb
					.Select(source =>
					{
						var ruleSet = new List<AccessRights>() { globalRule };

						if (directAccessToSource.TryGetValue(source.Id, out var directRule))
							ruleSet.Add(directRule);

						var rulesFromGroups = userGroups
							.SelectMany(x => x.Sources.Where(s => s.Id == source.Id))
							.Select(x => x.Rule)
							.ToArray();

						ruleSet.AddRange(rulesFromGroups);

						var chosenRule = ruleSet.OrderByDescending(x => x.AccessType).First();

						return new
						{
							source.Id,
							Rule = chosenRule,
						};
					})
					.ToArray();

				var userTags = tagsList
					.Select(tag =>
					{
						var ruleSet = new List<AccessRights>() { globalRule };

						if (directAccessToTags.TryGetValue(tag.Id, out var directRule))
							ruleSet.Add(directRule);

						var rulesFromBlocks = userBlocks
							.Where(x => tag.Blocks.Contains(x.Id))
							.Select(x => x.Rule)
							.ToArray();
						ruleSet.AddRange(rulesFromBlocks);

						var rulesFromGroups = userGroups
							.SelectMany(x => x.Tags.Where(t => t.Id == tag.Id))
							.Select(x => x.Rule)
							.ToArray();
						ruleSet.AddRange(rulesFromGroups);

						var chosenRule = ruleSet.OrderByDescending(x => x.AccessType).First();

						return new
						{
							tag.Id,
							tag.Name,
							tag.Guid,
							Rule = chosenRule,
						};
					})
					.ToArray();

				return new UserAuthInfo
				{
					Guid = user.Guid,
					FullName = user.Name,
					Token = string.Empty,
					GlobalAccessType = globalRule.AccessType,
					Groups = userGroups
						.ToDictionary(x => x.Guid, x => new AccessRuleInfo { RuleId = x.Rule.Id, AccessType = x.Rule.AccessType, }),
					Sources = userSources
						.ToDictionary(x => x.Id, x => new AccessRuleInfo { RuleId = x.Rule.Id, AccessType = x.Rule.AccessType, }),
					Blocks = userBlocks
						.ToDictionary(x => x.Id, x => new AccessRuleInfo { RuleId = x.Rule.Id, AccessType = x.Rule.AccessType, }),
					Tags = userTags
						.ToDictionary(x => x.Guid, x => new AccessRuleInfo { RuleId = x.Rule.Id, AccessType = x.Rule.AccessType, }),
				};
			})
			.ToDictionary(x => x.Guid);

		lock (locker)
		{
			UserRights = new ConcurrentDictionary<Guid, UserAuthInfo>(userRights);
		}
	}

	static object locker = new();

	#endregion

	#region Получение информации о учетной записи

	/// <summary>
	/// Получение информации о пользователе по идентификатору
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <returns>Информация о пользователе, включая доступ</returns>
	internal static UserAuthInfo GetAuthInfo(
		User user)
	{
		if (UserRights.TryGetValue(user.Guid, out var accessRights))
		{
			return new UserAuthInfo
			{
				Guid = user.Guid,
				FullName = user.FullName ?? "",
				Token = user.Type == UserType.Static ? (user.PasswordHash ?? string.Empty) : string.Empty,
				GlobalAccessType = accessRights.GlobalAccessType,
				Sources = accessRights.Sources,
				Blocks = accessRights.Blocks,
				Tags = accessRights.Tags,
				Groups = accessRights.Groups,
			};
		}
		else
			throw Errors.NoAccess;
	}

	/// <summary>
	/// Получение информации о пользователе EnergoId по его идентификатору<br/>
	/// Используется при пробросе действий из других приложений<br/>
	/// Позволяет совершать действия от имени другой учетной записи
	/// </summary>
	/// <param name="energoId">Идентификатор пользователя EnergoId</param>
	/// <returns>Информация о доступе учетной записи</returns>
	internal static UserAuthInfo GetAuthInfo(
		Guid energoId)
	{
		if (UserRights.TryGetValue(energoId, out var accessRights))
		{
			return accessRights;
		}
		else
			throw Errors.NoAccess;
	}

	#endregion

	#region Проверки прав доступа

	/// <summary>
	/// Проверка достаточности глобального уровня доступа
	/// </summary>
	/// <param name="user"></param>
	/// <param name="minimalAccess"></param>
	/// <returns></returns>
	public static bool HasGlobalAccess(
		UserAuthInfo user,
		AccessType minimalAccess)
	{
		bool access = user.GlobalAccessType.HasAccess(minimalAccess);
		if (user.UnderlyingUserGuid.HasValue)
		{
			var underlyingUser = GetAuthInfo(user.UnderlyingUserGuid.Value);
			access = access && HasGlobalAccess(underlyingUser, minimalAccess);
		}

		return access;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к источнику данных
	/// </summary>
	/// <param name="user"></param>
	/// <param name="minimalAccess"></param>
	/// <param name="sourceId"></param>
	/// <returns></returns>
	public static bool HasAccessToSource(
		UserAuthInfo user,
		AccessType minimalAccess,
		int sourceId)
	{
		if (!user.Sources.TryGetValue(sourceId, out var rule))
			return false;

		bool access = rule.AccessType.HasAccess(minimalAccess);
		if (user.UnderlyingUserGuid.HasValue)
		{
			var underlyingUser = GetAuthInfo(user.UnderlyingUserGuid.Value);
			access = access && HasAccessToSource(underlyingUser, minimalAccess, sourceId);
		}

		return access;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к блоку
	/// </summary>
	/// <param name="user"></param>
	/// <param name="minimalAccess"></param>
	/// <param name="blockId"></param>
	/// <returns></returns>
	public static bool HasAccessToBlock(
		UserAuthInfo user,
		AccessType minimalAccess,
		int blockId)
	{
		if (!user.Sources.TryGetValue(blockId, out var rule))
			return false;

		bool access = rule.AccessType.HasAccess(minimalAccess);
		if (user.UnderlyingUserGuid.HasValue)
		{
			var underlyingUser = GetAuthInfo(user.UnderlyingUserGuid.Value);
			access = access && HasAccessToBlock(underlyingUser, minimalAccess, blockId);
		}

		return access;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к тегу
	/// </summary>
	/// <param name="user"></param>
	/// <param name="minimalAccess"></param>
	/// <param name="guid"></param>
	/// <returns></returns>
	public static bool HasAccessToTag(
		UserAuthInfo user,
		AccessType minimalAccess,
		Guid guid)
	{
		if (!user.Tags.TryGetValue(guid, out var rule))
			return false;

		bool access = rule.AccessType.HasAccess(minimalAccess);
		if (user.UnderlyingUserGuid.HasValue)
		{
			var underlyingUser = GetAuthInfo(user.UnderlyingUserGuid.Value);
			access = access && HasAccessToTag(underlyingUser, minimalAccess, guid);
		}

		return access;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к группе пользователей
	/// </summary>
	/// <param name="userRights"></param>
	/// <param name="minimalAccess"></param>
	/// <param name="groupGuid"></param>
	/// <returns></returns>
	public static bool HasAccessToUserGroup(
		UserAuthInfo userRights,
		AccessType minimalAccess,
		Guid groupGuid)
	{
		if (!userRights.Groups.TryGetValue(groupGuid, out var rule))
			return false;

		return rule.AccessType.HasAccess(minimalAccess);
	}

	/// <summary>
	/// Проверка достаточности глобального уровня доступа
	/// </summary>
	/// <param name="user"></param>
	/// <param name="minimalAccess"></param>
	/// <exception cref="ForbiddenException">Нет доступа</exception>
	public static void ThrowIfNoGlobalAccess(
		UserAuthInfo user,
		AccessType minimalAccess)
	{
		if (!HasGlobalAccess(user, minimalAccess))
			throw Errors.NoAccess;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к источнику данных
	/// </summary>
	/// <param name="user"></param>
	/// <param name="minimalAccess"></param>
	/// <param name="sourceId"></param>
	/// <exception cref="ForbiddenException">Нет доступа</exception>
	public static void ThrowIfNoAccessToSource(
		UserAuthInfo user,
		AccessType minimalAccess,
		int sourceId)
	{
		if (!HasAccessToSource(user, minimalAccess, sourceId))
			throw Errors.NoAccess;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к блоку
	/// </summary>
	/// <param name="user"></param>
	/// <param name="minimalAccess"></param>
	/// <param name="blockId"></param>
	/// <exception cref="ForbiddenException">Нет доступа</exception>
	public static void ThrowIfNoAccessToBlock(
		UserAuthInfo user,
		AccessType minimalAccess,
		int blockId)
	{
		if (!HasAccessToBlock(user, minimalAccess, blockId))
			throw Errors.NoAccess;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к тегу
	/// </summary>
	/// <param name="user"></param>
	/// <param name="minimalAccess"></param>
	/// <param name="guid"></param>
	/// <exception cref="ForbiddenException">Нет доступа</exception>
	public static void ThrowIfNoAccessToTag(
		UserAuthInfo user,
		AccessType minimalAccess,
		Guid guid)
	{
		if (!HasAccessToTag(user, minimalAccess, guid))
			throw Errors.NoAccess;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к группе пользователей
	/// </summary>
	/// <param name="user"></param>
	/// <param name="minimalAccess"></param>
	/// <param name="groupGuid"></param>
	/// <exception cref="ForbiddenException">Нет доступа</exception>
	public static void ThrowIfNoAccessToUserGroup(
		UserAuthInfo user,
		AccessType minimalAccess,
		Guid groupGuid)
	{
		if (!HasAccessToUserGroup(user, minimalAccess, groupGuid))
			throw Errors.NoAccess;
	}
	
	/// <summary>
	 /// Получение глобального уровня доступа
	 /// </summary>
	 /// <param name="user">Информация о пользователе</param>
	 /// <returns>Глобальный уровень доступа</returns>
	public static AccessType GetGlobalAccess(UserAuthInfo user)
	{
		var access = user.GlobalAccessType;
		if (user.UnderlyingUserGuid.HasValue)
		{
			var underlyingUser = GetAuthInfo(user.UnderlyingUserGuid.Value);
			access = underlyingUser.GlobalAccessType;
		}

		return access;
	}

	/// <summary>
	/// Получение правила доступа к источнику данных
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <returns>Правило доступа</returns>
	public static AccessRuleInfo GetAccessToSource(
		UserAuthInfo user,
		int sourceId)
	{
		if (!user.Sources.TryGetValue(sourceId, out var rule))
			return AccessRuleInfo.Default;

		if (user.UnderlyingUserGuid.HasValue)
		{
			var underlyingUser = GetAuthInfo(user.UnderlyingUserGuid.Value);
			if (!underlyingUser.Sources.TryGetValue(sourceId, out rule))
				return AccessRuleInfo.Default;
		}

		return rule;
	}

	/// <summary>
	/// Получение правила доступа к блоку
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="blockId">Идентификатор блока</param>
	/// <returns>Правило доступа</returns>
	public static AccessRuleInfo GetAccessToBlock(
		UserAuthInfo user,
		int blockId)
	{
		if (!user.Blocks.TryGetValue(blockId, out var rule))
			return AccessRuleInfo.Default;

		if (user.UnderlyingUserGuid.HasValue)
		{
			var underlyingUser = GetAuthInfo(user.UnderlyingUserGuid.Value);
			if (!underlyingUser.Blocks.TryGetValue(blockId, out rule))
				return AccessRuleInfo.Default;
		}

		return rule;
	}

	/// <summary>
	/// Получение правила доступа к тегу
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="tagGuid">Идентификатор тега</param>
	/// <returns>Правило доступа</returns>
	public static AccessRuleInfo GetAccessToTag(
		UserAuthInfo user,
		Guid tagGuid)
	{
		if (!user.Tags.TryGetValue(tagGuid, out var rule))
			return AccessRuleInfo.Default;

		if (user.UnderlyingUserGuid.HasValue)
		{
			var underlyingUser = GetAuthInfo(user.UnderlyingUserGuid.Value);
			if (!underlyingUser.Tags.TryGetValue(tagGuid, out rule))
				return AccessRuleInfo.Default;
		}

		return rule;
	}

	/// <summary>
	/// Получение правила доступа к группе пользователей
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <returns>Правило доступа</returns>
	public static AccessRuleInfo GetAccessToUserGroup(
		UserAuthInfo user,
		Guid groupGuid)
	{
		if (!user.Groups.TryGetValue(groupGuid, out var rule))
			return AccessRuleInfo.Default;

		if (user.UnderlyingUserGuid.HasValue)
		{
			var underlyingUser = GetAuthInfo(user.UnderlyingUserGuid.Value);
			if (!underlyingUser.Groups.TryGetValue(groupGuid, out rule))
				return AccessRuleInfo.Default;
		}

		return rule;
	}

	#endregion

	#region Запросы

	/// <summary>
	/// Получение списка прав доступа
	/// </summary>
	internal IQueryable<AccessRightsInfo> QueryRights(
		Guid? userGuid = null,
		Guid? userGroupGuid = null,
		int? sourceId = null,
		int? blockId = null,
		int? tagId = null)
	{
		var rightsQuery = db.AccessRights
			.Where(x => userGuid == null || x.UserGuid == userGuid)
			.Where(x => userGroupGuid == null || x.UserGroupGuid == userGroupGuid)
			.Where(x => sourceId == null || x.SourceId == sourceId)
			.Where(x => blockId == null || x.BlockId == blockId)
			.Where(x => tagId == null || x.TagId == tagId);

		var query =
			from rights in rightsQuery
			from user in db.Users.LeftJoin(x => x.Guid == rights.UserGuid)
			from usergroup in db.UserGroups.LeftJoin(x => x.Guid == rights.UserGroupGuid)
			from source in db.Sources.LeftJoin(x => x.Id == rights.SourceId)
			from block in db.Blocks.LeftJoin(x => x.Id == rights.BlockId)
			from tag in db.Tags.LeftJoin(x => x.Id == rights.TagId)
			select new AccessRightsInfo
			{
				Id = rights.Id,
				AccessType = rights.AccessType,
				IsGlobal = rights.IsGlobal,
				Source = source == null ? null : new SourceSimpleInfo
				{
					Id = source.Id,
					Name = source.Name,
				},
				User = user == null ? null : new UserSimpleInfo
				{
					Guid = user.Guid,
					FullName = user.FullName ?? string.Empty,
				},
				UserGroup = usergroup == null ? null : new UserGroupSimpleInfo
				{
					Guid = usergroup.Guid,
					Name = usergroup.Name,
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
			};

		return query;
	}

	#endregion
}
