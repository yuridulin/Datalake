using Datalake.ApiClasses.Constants;
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
			.Where(x => groups.Where(g => g.Guid == x.UserGroupGuid).Any() || x.UserGuid == userGuid)
			.Where(x => x.AccessType != AccessType.NotSet)
			.ToArrayAsync();

		return rights;
	}

	public async Task<List<UserGroupsInfo>> GetUserGroupsAsync(
		Guid? userGuid)
	{
		var groupsTree = await GetUserGroupsTreeAsync(userGuid);
		var groups = new List<UserGroupsInfo>();

		foreach (var group in groupsTree)
		{
			ExtractGroups(group);
		}

		void ExtractGroups(UserGroupsTreeInfo userGroupInfo)
		{
			groups.Add(userGroupInfo);
			foreach (var child in userGroupInfo.Children)
				ExtractGroups(child);
		}

		return groups;
	}

	public async Task<UserGroupsTreeInfo[]> GetUserGroupsTreeAsync(
		Guid? userGuid)
	{
		if (userGuid == null)
			throw new NotFoundException(message: "пользователь без идентификатора");

		var user = await db.Users.FirstOrDefaultAsync(x => x.Guid == userGuid)
			?? throw new NotFoundException(message: "пользователь " + userGuid);

		var groupsQuery = from userGroup in db.UserGroups
											from rel in db.UserGroupRelations
											 .Where(x => x.UserGuid == userGuid)
											 .LeftJoin(x => x.UserGroupGuid == userGroup.Guid)
											group new { userGroup, rel } by userGroup into g
											select new
											{
												Id = g.Key.Guid,
												ParentId = g.Key.ParentGuid,
												g.Key.Name,
												Relations = g.Select(x => x.rel != null ? x.rel.AccessType : AccessType.NoAccess).ToArray(),
											};

		var groups = groupsQuery.ToArray();

		var userGroups = groups.Where(x => x.Relations.Intersect(EnumSet.UserWithAccess).Any()).ToArray();

		return userGroups
			.Select(x => new UserGroupsTreeInfo
			{
				Guid = x.Id,
				Name = x.Name,
				Children = ReadChildren(x.Id),
			})
			.ToArray();

		UserGroupsTreeInfo[] ReadChildren(Guid? id)
		{
			return groups
				.Where(x => x.ParentId == id)
				.Select(x => new UserGroupsTreeInfo
				{
					Name = x.Name,
					Guid = x.Id,
					Children = ReadChildren(id)
				})
				.ToArray();
		}
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
