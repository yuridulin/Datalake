using DatalakeApiClasses.Enums;
using DatalakeApiClasses.Exceptions;
using DatalakeApiClasses.Models.UserGroups;
using DatalakeApiClasses.Models.Users;
using DatalakeDatabase.Extensions;
using DatalakeDatabase.Models;
using DatalakeDatabase.Repositories.Base;
using LinqToDB;
using LinqToDB.Data;

namespace DatalakeDatabase.Repositories;

public partial class UserGroupsRepository(DatalakeContext db) : RepositoryBase
{
	#region Действия

	public async Task<Guid> CreateAsync(UserAuthInfo user, CreateUserGroupRequest request)
	{
		if (request.ParentGroupGuid.HasValue)
		{
			await CheckAccessToUserGroupAsync(db, user, AccessType.Admin, request.ParentGroupGuid.Value);
		}
		else
		{
			CheckGlobalAccess(user, AccessType.Admin);
		}

		return await CreateAsync(request);
	}

	public async Task<bool> UpdateAsync(UserAuthInfo user, Guid groupGuid, UpdateUserGroupRequest request)
	{
		await CheckAccessToUserGroupAsync(db, user, AccessType.Admin, groupGuid);

		return await UpdateAsync(groupGuid, request);
	}

	public async Task<bool> DeleteAsync(UserAuthInfo user, Guid groupGuid)
	{
		await CheckAccessToUserGroupAsync(db, user, AccessType.Admin, groupGuid);

		return await DeleteAsync(groupGuid);
	}

	#endregion


	#region Реализация

	internal async Task<Guid> CreateAsync(CreateUserGroupRequest request)
	{
		if (await db.UserGroups.AnyAsync(x => x.Name == request.Name && x.ParentGroupGuid == request.ParentGroupGuid))
			throw new AlreadyExistException(message: "группа " + request.Name);

		using var transaction = await db.BeginTransactionAsync();

		var group = await db.UserGroups
			.Value(x => x.Name, request.Name)
			.Value(x => x.ParentGroupGuid, request.ParentGroupGuid)
			.Value(x => x.Description, request.Description)
			.InsertWithOutputAsync();

		await db.AccessRights
			.Value(x => x.UserGroupGuid, group.UserGroupGuid)
			.Value(x => x.IsGlobal, true)
			.Value(x => x.AccessType, AccessType.Viewer)
			.InsertAsync();

		await db.LogAsync(Success(group.UserGroupGuid, $"Создана группа пользователей \"{group.Name}\""));
		await db.SetLastUpdateToNowAsync();
		await transaction.CommitAsync();

		return group.UserGroupGuid;
	}

	internal async Task<bool> UpdateAsync(Guid groupGuid, UpdateUserGroupRequest request)
	{
		if (await db.UserGroups.AnyAsync(x => x.Name == request.Name
			&& x.ParentGroupGuid == request.ParentGroupGuid
			&& x.UserGroupGuid != groupGuid))
			throw new AlreadyExistException(message: "группа " + request.Name);

		using var transaction = await db.BeginTransactionAsync();

		await db.UserGroups
			.Where(x => x.UserGroupGuid == groupGuid)
			.Set(x => x.Name, request.Name)
			.Set(x => x.Description, request.Description)
			.Set(x => x.ParentGroupGuid, request.ParentGroupGuid)
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
				UserGuid = u.UserGuid,
				UserGroupGuid = groupGuid,
				AccessType = u.AccessType,
			}));

		await db.LogAsync(Success(groupGuid, $"Изменена группа пользователей \"{groupGuid}\""));
		await db.SetLastUpdateToNowAsync();
		await transaction.CommitAsync();

		return true;
	}

	internal async Task<bool> DeleteAsync(Guid groupGuid)
	{
		using var transaction = await db.BeginTransactionAsync();

		var group = await db.UserGroups
			.FirstOrDefaultAsync(x => x.UserGroupGuid == groupGuid)
			?? throw new NotFoundException(message: "группа " + groupGuid);

		await db.AccessRights
			.Where(x => x.UserGroupGuid == groupGuid)
			.DeleteAsync();

		await db.UserGroupRelations
			.Where(x => x.UserGroupGuid == groupGuid)
			.DeleteAsync();

		await db.UserGroups
			.Where(x => x.UserGroupGuid == groupGuid)
			.DeleteAsync();

		await db.LogAsync(Success(groupGuid, $"Удалена группа пользователей \"{groupGuid}\""));
		await db.SetLastUpdateToNowAsync();
		await transaction.CommitAsync();

		return true;
	}

	internal static async Task CheckAccessToUserGroupAsync(
		DatalakeContext db,
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
				.Where(x => groups.Select(g => g.UserGroupGuid).Contains(groupGuid) && (int)minimalAccess <= (int)x.AccessType)
				.Any();
		}

		if (!hasAccess)
			throw NoAccess;
	}

	private static Log Success(Guid guid, string message, string? details = null) => new()
	{
		Category = LogCategory.UserGroups,
		RefId = guid.ToString(),
		Text = message,
		Type = LogType.Success,
		Details = details,
	};

	#endregion
}
