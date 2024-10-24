using Datalake.Database.Enums;
using Datalake.Database.Exceptions;
using Datalake.Database.Extensions;
using Datalake.Database.Models.UserGroups;
using Datalake.Database.Models.Users;
using Datalake.Database.Tables;
using LinqToDB;
using LinqToDB.Data;

namespace Datalake.Database.Repositories;

public partial class UserGroupsRepository(DatalakeContext db)
{
	#region Действия

	public async Task<Guid> CreateAsync(UserAuthInfo user, UserGroupCreateRequest request)
	{
		if (request.ParentGuid.HasValue)
		{
			await db.AccessRepository.CheckAccessToUserGroupAsync(user, AccessType.Admin, request.ParentGuid.Value);
		}
		else
		{
			await db.AccessRepository.CheckGlobalAccess(user, AccessType.Admin);
		}
		User = user.Guid;

		return await CreateAsync(request);
	}

	public async Task<bool> UpdateAsync(UserAuthInfo user, Guid groupGuid, UserGroupUpdateRequest request)
	{
		await db.AccessRepository.CheckAccessToUserGroupAsync(user, AccessType.Admin, groupGuid);
		User = user.Guid;

		return await UpdateAsync(groupGuid, request);
	}

	public async Task<bool> MoveAsync(UserAuthInfo user, Guid guid, Guid? parentGuid)
	{
		await db.AccessRepository.CheckAccessToUserGroupAsync(user, AccessType.Admin, guid);

		if (parentGuid.HasValue)
		{
			await db.AccessRepository.CheckAccessToUserGroupAsync(user, AccessType.User, parentGuid.Value);
		}
		else
		{
			await db.AccessRepository.CheckGlobalAccess(user, AccessType.User);
		}
		User = user.Guid;

		return await MoveAsync(guid, parentGuid);
	}

	public async Task<bool> DeleteAsync(UserAuthInfo user, Guid groupGuid)
	{
		await db.AccessRepository.CheckAccessToUserGroupAsync(user, AccessType.Admin, groupGuid);
		User = user.Guid;

		return await DeleteAsync(groupGuid);
	}

	#endregion


	#region Реализация

	Guid User { get; set; }

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

		SystemRepository.Update();

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
			.Set(x => x.ParentGuid, request.ParentGuid)
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

		SystemRepository.Update();

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

		SystemRepository.Update();

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
}
