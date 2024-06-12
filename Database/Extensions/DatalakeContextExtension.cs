using Datalake.ApiClasses.Constants;
using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Exceptions;
using Datalake.ApiClasses.Models.Users;
using Datalake.Database.Models;
using Datalake.Database.Repositories;
using LinqToDB;
using System.Linq;

namespace Datalake.Database.Extensions;

public static class DatalakeContextExtension
{
	/// <summary>
	/// Обновление времени последнего изменения структуры тегов, источников и сущностей в базе данных
	/// </summary>
	/// <param name="db">Подключение к базе данных</param>
	public static async Task SetLastUpdateToNowAsync(this DatalakeContext db)
	{
		await db.Settings
			.Set(x => x.LastUpdate, DateTime.UtcNow)
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
	/// <returns>Объект разрешений пользователя</returns>
	public static async Task<AccessRights[]> AuthorizeUserAsync(
		this DatalakeContext db,
		Guid? userGuid)
	{
		var groups = await db.GetUserGroupsAsync(userGuid);

		var rights = await db.AccessRights
			.Where(x => groups.Where(g => g.Guid == x.UserGroupGuid).Any() || x.UserGuid == userGuid)
			.Where(x => x.AccessType != AccessType.NotSet)
			.ToArrayAsync();

		return rights;
	}

	public static async Task<List<UserGroupsInfo>> GetUserGroupsAsync(
		this DatalakeContext db,
		Guid? userGuid)
	{
		var groupsTree = await db.GetUserGroupsTreeAsync(userGuid);
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

	public static async Task<UserGroupsTreeInfo[]> GetUserGroupsTreeAsync(
		this DatalakeContext db,
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
}
