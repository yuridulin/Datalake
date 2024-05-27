using DatalakeApiClasses.Constants;
using DatalakeApiClasses.Enums;
using DatalakeApiClasses.Models.Abstractions;
using DatalakeApiClasses.Models.Users;
using DatalakeDatabase.Models;
using LinqToDB;
using System.Linq.Expressions;

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
	public static async Task<IRights> AuthorizeUser(
		this DatalakeContext db,
		Guid userGuid,
		Expression<Func<AccessRights, bool>>? objectWhere = null)
	{
		var groups = await db.GetUserGroupsAsync(userGuid);

		var rightsArrayQuery = db.AccessRights
			.Where(x => groups.Select(g => g.Guid).Contains(x.UserGroupGuid.ToString()) || x.UserGuid == userGuid);

		if (objectWhere != null)
		{
			rightsArrayQuery = rightsArrayQuery.Where(objectWhere);
		}

		var rightsArray = await rightsArrayQuery.ToArrayAsync();

		return rightsArray.Merge();
	}

	public static async Task<List<UserGroupInfo>> GetUserGroupsAsync(
		this DatalakeContext db,
		Guid userGuid)
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
		Guid userGuid)
	{
		var user = await db.Users.FirstOrDefaultAsync(x => x.UserGuid == userGuid);
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
												Relations = g.Select(x => x.rel != null ? x.rel.AccessType : UserGroupAccess.Not).ToArray(),
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
}
