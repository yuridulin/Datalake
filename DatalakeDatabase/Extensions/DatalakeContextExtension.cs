using DatalakeApiClasses.Constants;
using DatalakeApiClasses.Enums;
using DatalakeApiClasses.Exceptions;
using DatalakeApiClasses.Models.Users;
using DatalakeDatabase.Models;
using DatalakeDatabase.Repositories;
using LinqToDB;

namespace DatalakeDatabase.Extensions;

public static class DatalakeContextExtension
{
	/// <summary>
	/// Обновление времени последнего изменения структуры тегов, источников и сущностей в базе данных
	/// </summary>
	/// <param name="db">Подключение к базе данных</param>
	public static async Task SetLastUpdateToNowAsync(this DatalakeContext db)
	{
		await db.Settings
			.Set(x => x.LastUpdate, DateTime.Now)
			.UpdateAsync();
	}

	public static async Task<DateTime> GetLastUpdateAsync(this DatalakeContext db)
	{
		var lastUpdate = await db.Settings
			.Select(x => x.LastUpdate)
			.DefaultIfEmpty(DateTime.MinValue)
			.FirstOrDefaultAsync();

		return lastUpdate;
	}

	public static async Task LogAsync(
		this DatalakeContext db,
		Log log)
	{
		try
		{
			await db.InsertAsync(log);
		}
		catch { }
	}

	/// <summary>
	/// Авторизация пользователя по его ключу, включая все группы, в которых он состоит
	/// </summary>
	/// <param name="db">Подключение к базе данных</param>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <param name="objectWhere">Дополнительные условия проверки</param>
	/// <returns>Объект разрешений пользователя</returns>
	public static async Task<AccessRights[]> AuthorizeUserAsync(
		this DatalakeContext db,
		Guid? userGuid)
	{
		var groups = await db.GetUserGroupsAsync(userGuid);

		var rights = await db.AccessRights
			.Where(x => groups.Select(g => g.Guid).Contains(x.UserGroupGuid.ToString()) || x.UserGuid == userGuid)
			.Where(x => x.AccessType != AccessType.NotSet)
			.ToArrayAsync();

		return rights;
	}

	public static async Task<List<UserGroupInfo>> GetUserGroupsAsync(
		this DatalakeContext db,
		Guid? userGuid)
	{
		var groupsTree = await db.GetUserGroupsTreeAsync(userGuid);
		var groups = new List<UserGroupInfo>();

		foreach (var group in groupsTree)
		{
			ExtractGroups(group);
		}

		void ExtractGroups(UserGroupTreeInfo userGroupInfo)
		{
			groups.Add(userGroupInfo);
			foreach (var child in userGroupInfo.Children)
				ExtractGroups(child);
		}

		return groups;
	}

	public static async Task<UserGroupTreeInfo[]> GetUserGroupsTreeAsync(
		this DatalakeContext db,
		Guid? userGuid)
	{
		if (userGuid == null)
			throw new NotFoundException(message: "пользователь без идентификатора");

		var user = await db.Users.FirstOrDefaultAsync(x => x.UserGuid == userGuid)
			?? throw new NotFoundException(message: "пользователь " + userGuid);

		var groupsQuery = from userGroup in db.UserGroups
											from rel in db.UserGroupRelations
											 .Where(x => x.UserGuid == userGuid)
											 .LeftJoin(x => x.UserGroupGuid == userGroup.UserGroupGuid)
											group new { userGroup, rel } by userGroup into g
											select new
											{
												Id = g.Key.UserGroupGuid.ToString(),
												ParentId = g.Key.ParentGroupGuid.ToString(),
												g.Key.Name,
												Relations = g.Select(x => x.rel != null ? x.rel.AccessType : AccessType.NoAccess).ToArray(),
											};

		var groups = groupsQuery.ToArray();

		var userGroups = groups.Where(x => x.Relations.Intersect(EnumSet.UserWithAccess).Any()).ToArray();

		return userGroups
			.Select(x => new UserGroupTreeInfo
			{
				Guid = x.Id,
				Name = x.Name,
				Children = ReadChildren(x.Id),
			})
			.ToArray();

		UserGroupTreeInfo[] ReadChildren(string? id)
		{
			return groups
				.Where(x => x.ParentId == id)
				.Select(x => new UserGroupTreeInfo
				{
					Name = x.Name,
					Guid = x.Id,
					Children = ReadChildren(id)
				})
				.ToArray();
		}
	}

	/// <summary>
	/// Проверка, есть ли доступ на указанном уровне
	/// </summary>
	/// <param name="db">Подключение к базе данных</param>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <param name="minimalAccess">Уровень доступа, минимально необходимый для предоставления разрешения</param>
	/// <param name="objectWhere">Условие выбора объектов. Глобальные разрешения пользователя и его групп всегда в списке выбранных</param>
	/// <exception cref="ForbiddenException">Нет доступа</exception>
	public static async Task CheckAccessAsync(
		this DatalakeContext db,
		UserAuthInfo user,
		AccessType minimalAccess,
		AccessScope scope = AccessScope.Global,
		int targetId = 0)
	{
		var query = user.Rights
			.Where(x => (int)minimalAccess <= (int)x.AccessType);

		if (scope != AccessScope.Global)
		{
			List<int> allowedBlocks = [];
			int allowedSource = -1;
			int allowedTag = -1;

			switch (scope)
			{
				case AccessScope.Source:
					allowedSource  = targetId;
					break;

				case AccessScope.Block:
					allowedBlocks.AddRange((await new BlocksRepository(db).GetParentsAsync(targetId)).Select(b => b.Id));
					break;

				case AccessScope.Tag:
					var sourceQuery = from t in db.Tags.Where(x => x.Id == targetId)
														from s in db.Sources.InnerJoin(x => x.Id == t.SourceId)
														select s.Id;
					allowedSource = await sourceQuery.DefaultIfEmpty(-1).FirstOrDefaultAsync();

					var blocksQuery = from rel in db.BlockTags.Where(x => x.TagId == targetId)
														from b in db.Blocks.InnerJoin(x => x.Id == rel.BlockId)
														select b.Id;
					allowedBlocks.AddRange(await blocksQuery.ToArrayAsync());
					allowedBlocks.AddRange((await new BlocksRepository(db).GetParentsAsync(targetId)).Select(b => b.Id));

					allowedTag = targetId;
					break;
			}

			query = query.Where(x => x.IsGlobal
			  || allowedBlocks.Contains(x.BlockId ?? -1)
				|| x.SourceId == allowedSource
				|| x.TagId == allowedTag);
		}

		var hasAccess = query.Any();

		if (!hasAccess)
			throw new ForbiddenException(message: "нет доступа");
	}
}
