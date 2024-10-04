using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Exceptions;
using Datalake.ApiClasses.Models.Users;
using Datalake.Database.Models;
using LinqToDB;

namespace Datalake.Database.Repositories.Base;

/// <summary>
/// База репозитория содержит методы проверки прав и получения информации о пользователях
/// для разграничения доступа к работе с БД
/// </summary>
public abstract class RepositoryBase(DatalakeContext context)
{
	protected readonly DatalakeContext db = context;

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

	protected async Task<UserAuthInfo> GetAuthInfo(
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

	protected async Task CheckGlobalAccess(
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

	protected async Task CheckAccessToSource(
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

	protected async Task CheckAccessToBlockAsync(
		UserAuthInfo user,
		AccessType minimalAccess,
		int blockId,
		Guid? energoId = null)
	{
		// TODO: Сам себя получается вызывает. Что за нахуй?
		var blockWithParents = await new BlocksRepository(db).GetWithParentsAsync(blockId);
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

	protected async Task CheckAccessToTagAsync(
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
		var repository = new BlocksRepository(db); // TODO: Сам себя получается вызывает. Что за нахуй? bonus х2
		var blocksHasThisTagWithParents = blocksHasThisTag
			.SelectMany(x =>
			{
				var blockWithParents = repository.GetWithParentsAsync(x).Result;
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

	protected static readonly ForbiddenException NoAccess = new(message: "нет доступа");

	#endregion
}
