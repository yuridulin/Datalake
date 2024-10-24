using Datalake.Database.Enums;
using Datalake.Database.Exceptions;
using Datalake.Database.Models.AccessRights;
using Datalake.Database.Models.Users;
using Datalake.Database.Tables;
using LinqToDB;
using LinqToDB.Data;

namespace Datalake.Database.Repositories;

public partial class AccessRepository(DatalakeContext db)
{
	#region Действия

	public async Task<UserAuthInfo> AuthenticateAsync(UserEnergoIdInfo info)
	{
		var user = await db.Users
			.Where(x => x.Type == UserType.EnergoId)
			.Where(x => x.EnergoIdGuid != null && x.EnergoIdGuid == info.EnergoIdGuid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "указанная учётная запись по guid");

		return await GetAuthInfo(user);
	}

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

		return await GetAuthInfo(user);
	}

	public async Task ApplyChangesAsync(UserAuthInfo user, AccessRightsApplyRequest request)
	{
		await CheckGlobalAccess(user, AccessType.Admin);

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
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			throw new DatabaseException(message: "не удалось обновить права доступа", ex);
		}
	}

	#endregion

	/* 
	 * Нужны списки по тегам и блокам:
	 * 1. для каждого тега: список юзеров, которые имеют доступ, с указанием уровня. Высчитать на основе блоков и групп пользователей
	 * 2. для каждого блока: список юзеров, которые имеют доступ, с указанием уровня. Высчитать на основе блоков и групп пользователей
	 * 3. для каждого пользователя: список блоков и тегов, к которым есть доступ, с указанием уровня
	 * 
	 * Этот список создается в начале и пересчитывается, когда происходит любое изменение с уровнями доступа:
	 * 1. появляется новое правило (при создании тега, блока, выдаче разрешений)
	 * 2. изменяется правило (при изменении разрешений)
	 * 3. удаляется правило (при удалении разрешений либо объекта)
	 */

	#region Информация о учетной записи

	/// <summary>
	/// Получение информации о пользователе EnergoId по его идентификатору<br/>
	/// Используется при пробросе действий из других приложений<br/>
	/// Позволяет совершать действия от имени другой учетной записи
	/// </summary>
	/// <param name="energoId">Идентификатор пользователя EnergoId</param>
	/// <returns>Информация о учетной записи</returns>
	/// <exception cref="NotFoundException">Пользователь не найден</exception>
	protected async Task<UserAuthInfo> GetEnergoIdUserAsync(
		Guid energoId)
	{
		var user = await db.Users
			.Where(x => x.Type == UserType.EnergoId)
			.Where(x => x.EnergoIdGuid != null && x.EnergoIdGuid == energoId)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "указанная учётная запись по guid");

		return await GetAuthInfo(user);
	}

	internal async Task<UserAuthInfo> GetAuthInfo(
		User user)
	{
		var accessRights = await AuthorizeUserAsync(user.Guid);

		return new UserAuthInfo
		{
			Guid = user.Guid,
			FullName = user.FullName ?? "",
			Token = user.Type == UserType.Static ? (user.PasswordHash ?? string.Empty) : string.Empty,
			Rights = accessRights
				.Select(x => new UserAccessRightsInfo
				{
					AccessType = x.AccessType,
					IsGlobal = x.IsGlobal,
					BlockId = x.BlockId,
					SourceId = x.SourceId,
					TagId = x.TagId,
				})
				.ToArray(),
		};
	}

	/// <summary>
	/// Авторизация пользователя по его ключу, включая все группы, в которых он состоит
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <returns>Объект разрешений пользователя</returns>
	public async Task<AccessRights[]> AuthorizeUserAsync(
		Guid? userGuid)
	{
		var groups = await GetUserGroupsAsync(userGuid);

		var rights = await db.AccessRights
			.Where(x => groups.Where(g => g.GroupGuid == x.UserGroupGuid).Any() || x.UserGuid == userGuid)
			.Where(x => x.AccessType != AccessType.NotSet)
			.ToArrayAsync();

		return rights;
	}

	/// <summary>
	/// Получаем список всех групп, к которым у пользователя есть доступ, по его идентификатору
	/// Будут получены все группы с предоставленным доступом, а так же все дочерние
	/// Если для дочерней группы не указано отдельное разрешение, действует родительское
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <returns>Плоский список всех групп с указанием действующего уровня доступа для каждой</returns>
	public async Task<IEnumerable<UserAccessGroupInfo>> GetUserGroupsAsync(
		Guid? userGuid)
	{
		var userGroupsCTE = db.GetCte<UserAccessGroupCTE>(cte => (
				from ug in db.UserGroups
				from ugr in db.UserGroupRelations.LeftJoin(x => x.UserGroupGuid == ug.Guid)
				select new UserAccessGroupCTE
				{
					GroupGuid = ug.Guid,
					ParentGuid = ug.ParentGuid,
					AccessType = ugr.AccessType,
					UserGuid = ugr.UserGuid,
				})
				.Concat(
						from sub_ug in db.UserGroups
						from cteItem in cte.InnerJoin(x => x.GroupGuid == sub_ug.ParentGuid)
						from sub_ugr in db.UserGroupRelations.LeftJoin(x => x.UserGroupGuid == sub_ug.Guid).DefaultIfEmpty()
						select new UserAccessGroupCTE
						{
							GroupGuid = sub_ug.Guid,
							ParentGuid = sub_ug.ParentGuid,
							AccessType = sub_ugr != null ? sub_ugr.AccessType : cteItem.AccessType,
							UserGuid = sub_ugr != null ? sub_ugr.UserGuid : cteItem.UserGuid,
						}
				)
		);

		var userGroupsWithAccess = await userGroupsCTE
			.Where(x => x.UserGuid == userGuid)
			.Select(x => new UserAccessGroupInfo { GroupGuid = x.GroupGuid, AccessType = x.AccessType, })
			.Distinct()
			.ToArrayAsync();

		return userGroupsWithAccess;
	}

	#endregion

	#region Проверки прав доступа

	internal async Task CheckGlobalAccess(
		UserAuthInfo user,
		AccessType minimalAccess,
		Guid? energoId = null)
	{
		var hasAccess = user.Rights
			.Where(x => (int)minimalAccess <= (int)x.AccessType)
			.Where(x => x.IsGlobal)
			.Any();

		if (!hasAccess)
			throw NoAccess;

		if (energoId.HasValue)
		{
			var energoIdUser = await GetEnergoIdUserAsync(energoId.Value);
			await CheckGlobalAccess(energoIdUser, minimalAccess);
		}
	}

	internal async Task CheckAccessToSource(
		UserAuthInfo user,
		AccessType minimalAccess,
		int sourceId,
		Guid? energoId = null)
	{
		var hasAccess = user.Rights
			.Where(x => (int)minimalAccess <= (int)x.AccessType)
			.Where(x => x.IsGlobal || x.SourceId == sourceId)
			.Any();

		if (!hasAccess)
			throw NoAccess;

		if (energoId.HasValue)
		{
			var energoIdUser = await GetEnergoIdUserAsync(energoId.Value);
			await CheckAccessToSource(energoIdUser, minimalAccess, sourceId);
		}
	}

	internal async Task CheckAccessToBlockAsync(
		UserAuthInfo user,
		AccessType minimalAccess,
		int blockId,
		Guid? energoId = null)
	{
		var blockWithParents = await db.BlocksRepository.GetWithParentsAsync(blockId);
		var blocksId = blockWithParents.Select(x => x.Id).ToArray();

		var hasAccess = user.Rights
			.Where(x => (int)minimalAccess <= (int)x.AccessType)
			.Where(x => x.IsGlobal || blocksId.Contains(x.SourceId ?? -1))
			.Any();

		if (!hasAccess)
			throw NoAccess;

		if (energoId.HasValue)
		{
			var energoIdUser = await GetEnergoIdUserAsync(energoId.Value);
			await CheckAccessToBlockAsync(energoIdUser, minimalAccess, blockId);
		}
	}

	internal async Task CheckAccessToTagAsync(
		UserAuthInfo user,
		AccessType minimalAccess,
		Guid guid,
		Guid? energoId = null)
	{
		var sourceQuery =
			from t in db.Tags.Where(x => x.GlobalGuid == guid)
			from s in db.Sources.InnerJoin(x => x.Id == t.SourceId)
			select s.Id;

		var source = await sourceQuery
			.DefaultIfEmpty(-1)
			.FirstOrDefaultAsync();

		var blocksQuery =
			from t in db.Tags.Where(x => x.GlobalGuid == guid)
			from rel in db.BlockTags.Where(x => x.TagId == t.Id)
			from b in db.Blocks.InnerJoin(x => x.Id == rel.BlockId)
			select b.Id;

		var blocksHasThisTag = await blocksQuery.ToArrayAsync();
		var blocksHasThisTagWithParents = blocksHasThisTag
			.SelectMany(x =>
			{
				var blockWithParents = db.BlocksRepository.GetWithParentsAsync(x).Result;
				return blockWithParents.Select(b => b.Id).ToArray();
			})
			.ToList();

		var hasAccess = user.Rights
			.Where(x => (int)minimalAccess <= (int)x.AccessType)
			.Where(x => x.IsGlobal || blocksHasThisTagWithParents.Contains(x.TagId ?? -1))
			.Any();

		if (!hasAccess)
			throw NoAccess;

		if (energoId.HasValue)
		{
			var energoIdUser = await GetEnergoIdUserAsync(energoId.Value);
			await CheckAccessToTagAsync(energoIdUser, minimalAccess, guid);
		}
	}

	internal async Task CheckAccessToUserGroupAsync(
		UserAuthInfo user,
		AccessType minimalAccess,
		Guid groupGuid)
	{
		var hasAccess = user.Rights
			.Where(x => x.IsGlobal && (int)minimalAccess <= (int)x.AccessType)
			.Any();

		if (!hasAccess)
		{
			var groups = await new UserGroupsRepository(db).GetWithParentsAsync(groupGuid);
			hasAccess = user.Rights
				.Where(x => groups.Select(g => g.Guid).Contains(groupGuid) && (int)minimalAccess <= (int)x.AccessType)
				.Any();
		}

		if (!hasAccess)
			throw NoAccess;
	}

	static readonly ForbiddenException NoAccess = new(message: "нет доступа");

	#endregion
}
