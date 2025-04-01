using Datalake.Database.Constants;
using Datalake.Database.Extensions;
using Datalake.Database.Tables;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.UserGroups;
using Datalake.PublicApi.Models.Users;
using LinqToDB;
using System.Security.Cryptography;
using System.Text;

namespace Datalake.Database.Repositories;

/// <summary>
/// Репозиторий для работы с учетными записями пользователей
/// </summary>
public static class UsersRepository
{
	#region Действия

	/// <summary>
	/// Создание нового пользователя
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Идентификатор создающего пользователя</param>
	/// <param name="userInfo">Параметры новой учетной записи</param>
	/// <returns>Идентификатор созданного пользователя</returns>
	public static async Task<Guid> CreateAsync(
		DatalakeContext db, UserAuthInfo user, UserCreateRequest userInfo)
	{
		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		return await CreateAsync(db, user.Guid, userInfo);
	}

	/// <summary>
	/// Получение информации о пользователях
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Идентификатор читающего пользователя</param>
	/// <returns>Список пользователей</returns>
	public static async Task<UserInfo[]> ReadAllAsync(
		DatalakeContext db, UserAuthInfo user)
	{
		var users = await GetInfo(db)
			.ToArrayAsync();

		foreach (var u in users)
		{
			u.AccessRule = (user.Guid == u.Guid && !user.GlobalAccessType.HasAccess(AccessType.Manager))
				? new AccessRuleInfo { AccessType = AccessType.Manager }
				: new AccessRuleInfo { AccessType = user.GlobalAccessType, RuleId = 0 };

			if (!u.AccessRule.AccessType.HasAccess(AccessType.Manager))
			{
				u.FullName = string.Empty;
				u.AccessType = AccessType.NotSet;
				u.Login = string.Empty;
				u.Type = UserType.Local;
				u.UserGroups = [];
				u.Guid = Guid.Empty;
			}
		}

		return [.. users.Where(x => x.AccessRule.AccessType.HasAccess(AccessType.Viewer))];
	}

	/// <summary>
	/// Получение информации о пользователе
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Идентификатор читающего пользователя</param>
	/// <param name="guid">Идентификатор затронутого пользователя</param>
	/// <returns>Детальная о пользователе</returns>
	public static async Task<UserInfo> ReadAsync(
		DatalakeContext db, UserAuthInfo user, Guid guid)
	{
		if (user.Guid != guid)
			AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Viewer);

		var userInfo = await GetInfo(db)
			.Where(x => x.Guid == guid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"Учётная запись {guid}");

		foreach (var group in userInfo.UserGroups)
		{
			group.AccessRule = AccessRepository.GetAccessToUserGroup(user, group.Guid);
		}

		userInfo.AccessRule = (user.Guid == guid && !user.GlobalAccessType.HasAccess(AccessType.Manager))
			? new AccessRuleInfo { AccessType = AccessType.Manager, RuleId = 0 }
			: new AccessRuleInfo { AccessType = user.GlobalAccessType, RuleId = 0 };

		return userInfo;
	}

	/// <summary>
	/// Получение детальной информации о пользователе, включая группы и правила
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Идентификатор читающего пользователя</param>
	/// <param name="guid">Идентификатор затронутого пользователя</param>
	/// <returns>Детальная информация о пользователе</returns>
	public static async Task<UserDetailInfo> ReadWithDetailsAsync(
		DatalakeContext db, UserAuthInfo user, Guid guid)
	{
		if (user.Guid != guid)
			AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Viewer);

		var userInfo = await GetDetailInfo(db)
			.Where(x => x.Guid == guid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"Учётная запись {guid}");

		userInfo.AccessRule = (user.Guid == guid && !user.GlobalAccessType.HasAccess(AccessType.Manager))
			? new AccessRuleInfo { AccessType = AccessType.Manager, RuleId = 0 }
			: new AccessRuleInfo { AccessType = user.GlobalAccessType, RuleId = 0 };

		foreach (var group in userInfo.UserGroups)
		{
			group.AccessRule = AccessRepository.GetAccessToUserGroup(user, group.Guid);
		}

		return userInfo;
	}

	/// <summary>
	/// Изменение параметров пользователя
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Идентификатор изменяющего пользователя</param>
	/// <param name="userGuid">Идентификатор затронутого пользователя</param>
	/// <param name="request">Новые параметры учетной записи</param>
	/// <returns>Флаг успешного завершения</returns>
	public static async Task<bool> UpdateAsync(
		DatalakeContext db, UserAuthInfo user, Guid userGuid, UserUpdateRequest request)
	{
		var accessType = user.Guid == userGuid ? AccessType.Manager : user.GlobalAccessType;

		if (!accessType.HasAccess(AccessType.Manager))
			throw Errors.NoAccess;

		return await UpdateAsync(db, user.Guid, userGuid, request);
	}

	/// <summary>
	/// Удаление пользователя
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Идентификатор удаляющего пользователя</param>
	/// <param name="userGuid">Идентификатор затронутого пользователя</param>
	/// <returns>Флаг успешного завершения</returns>
	public static async Task<bool> DeleteAsync(
		DatalakeContext db, UserAuthInfo user, Guid userGuid)
	{
		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		return await DeleteAsync(db, user.Guid, userGuid);
	}

	#endregion

	#region Реализация

	internal static async Task<Guid> CreateAsync(
		DatalakeContext db, Guid userGuid, UserCreateRequest request)
	{
		string? hash = null;

		switch (request.Type)
		{
			case UserType.Local:
				if (string.IsNullOrEmpty(request.Login))
					throw new InvalidValueException(message: "логин не может быть пустым");
				if (await UsersNotDeleted(db).AnyAsync(x => x.Login == request.Login && !x.IsDeleted))
					throw new AlreadyExistException(message: "учётная запись с таким логином");
				if (string.IsNullOrEmpty(request.Password))
					throw new InvalidValueException(message: "необходимо указать пароль");
				hash = GetHashFromPassword(request.Password);
				break;

			case UserType.Static:
				hash = await GenerateNewHashForStaticAsync(db);
				request.Login = string.Empty;
				break;

			case UserType.EnergoId:
				if (request.EnergoIdGuid == null)
					throw new InvalidValueException(message: "необходимо указать учетную запись EnergoId");
				if (await UsersNotDeleted(db).AnyAsync(x => x.EnergoIdGuid == request.EnergoIdGuid && !x.IsDeleted))
					throw new AlreadyExistException(message: "учётная запись с выбранным EnergoID");
				hash = string.Empty;
				request.Login = string.Empty;
				break;
		}

		var createdUser = new User
		{
			Guid = Guid.NewGuid(),
			Login = request.Login,
			FullName = request.FullName ?? request.Login,
			PasswordHash = hash,
			StaticHost = request.StaticHost,
			EnergoIdGuid = request.EnergoIdGuid,
			Type = request.Type,
		};

		using var transaction = await db.BeginTransactionAsync();

		createdUser = await db.Users
			.Value(x => x.Guid, createdUser.Guid)
			.Value(x => x.Login, createdUser.Login)
			.Value(x => x.FullName, createdUser.FullName)
			.Value(x => x.PasswordHash, createdUser.PasswordHash)
			.Value(x => x.StaticHost, createdUser.StaticHost)
			.Value(x => x.EnergoIdGuid, createdUser.EnergoIdGuid)
			.Value(x => x.Type, createdUser.Type)
			.InsertWithOutputAsync();

		await db.AccessRights
			.Value(x => x.UserGuid, createdUser.Guid)
			.Value(x => x.IsGlobal, true)
			.Value(x => x.AccessType, request.AccessType)
			.InsertAsync();

		await LogAsync(db, userGuid, createdUser.Guid, "Создан пользователь " + createdUser.FullName);

		await transaction.CommitAsync();

		AccessRepository.Update();

		return createdUser.Guid;
	}

	internal static async Task<bool> UpdateAsync(
		DatalakeContext db, Guid userGuid, Guid affectedUserGuid, UserUpdateRequest request)
	{
		var oldUser = await UsersNotDeleted(db)
			.Where(x => x.Guid == affectedUserGuid && !x.IsDeleted)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "пользователь по указанному ключу");

		string? newHash = null;
		switch (request.Type)
		{
			case UserType.Local:
				if (string.IsNullOrEmpty(request.Login))
					throw new InvalidValueException(message: "логин не может быть пустым");
				if (await UsersNotDeleted(db).AnyAsync(x => x.Login == request.Login && x.Guid != affectedUserGuid && !x.IsDeleted))
					throw new AlreadyExistException(message: "учётная запись с таким логином");
				if (request.Type != oldUser.Type && string.IsNullOrEmpty(request.Password))
					throw new InvalidValueException(message: "при смене типа учетной записи необходимо задать пароль");
				if (!string.IsNullOrEmpty(request.Password))
					newHash = GetHashFromPassword(request.Password);
				break;

			case UserType.Static:
				request.Login ??= string.Empty;
				if (request.CreateNewStaticHash || request.Type != oldUser.Type)
					newHash = await GenerateNewHashForStaticAsync(db);
				break;

			case UserType.EnergoId:
				if (request.EnergoIdGuid == null)
					throw new InvalidValueException(message: "необходимо указать учетную запись EnergoId");
				if (await UsersNotDeleted(db).AnyAsync(x => x.EnergoIdGuid == request.EnergoIdGuid && x.Guid != affectedUserGuid && !x.IsDeleted))
					throw new AlreadyExistException(message: "учётная запись с выбранным EnergoID");
				break;
		}

		using var transaction = await db.BeginTransactionAsync();

		int updatedRows = await db.Users
			.Where(x => x.Guid == affectedUserGuid)
			.Set(x => x.Type, request.Type)
			.Set(x => x.Login, request.Login)
			.Set(x => x.FullName, request.FullName)
			.Set(x => x.EnergoIdGuid, request.EnergoIdGuid)
			.Set(x => x.PasswordHash, newHash ?? oldUser.PasswordHash)
			.Set(x => x.StaticHost, request.StaticHost ?? oldUser.StaticHost)
			.UpdateAsync();

		updatedRows += await db.AccessRights
			.Where(x => x.UserGuid == affectedUserGuid)
			.Set(x => x.AccessType, request.AccessType)
			.UpdateAsync();

		await LogAsync(db, userGuid, affectedUserGuid, "Изменен пользователь " + (request.FullName ?? request.Login), ObjectExtension.Difference(
			new { oldUser.Type, oldUser.Login, oldUser.FullName, oldUser.EnergoIdGuid, oldUser.StaticHost, Hash = "old" },
			new { request.Type, request.Login, request.FullName, request.EnergoIdGuid, request.StaticHost, Hash = string.IsNullOrEmpty(request.StaticHost) ? "new" : "old" }));

		await transaction.CommitAsync();

		AccessRepository.Update();

		return true;
	}

	internal static async Task<bool> DeleteAsync(
		DatalakeContext db, Guid userGuid, Guid affectedUserGuid)
	{
		using var transaction = await db.BeginTransactionAsync();

		var user = await UsersNotDeleted(db)
			.Where(x => x.Guid == affectedUserGuid && !x.IsDeleted)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "пользователь " + affectedUserGuid);

		/*await db.AccessRights
			.Where(x => x.UserGuid == affectedUserGuid)
			.DeleteAsync();

		await db.UserGroupRelations
			.Where(x => x.UserGuid == affectedUserGuid)
			.DeleteAsync();*/

		await db.Users
			.Where(x => x.Guid == affectedUserGuid)
			.Set(x => x.IsDeleted, true)
			.UpdateAsync();

		await LogAsync(db, userGuid, affectedUserGuid, "Удален пользователь " + (user.FullName ?? user.Login));

		await transaction.CommitAsync();

		AccessRepository.Update();

		return true;
	}

	internal static string GetHashFromPassword(string password)
	{
		if (string.IsNullOrEmpty(password))
			throw new InvalidValueException(message: "пароль не может быть пустым");

		var hash = SHA1.HashData(Encoding.UTF8.GetBytes(password));
		return Convert.ToBase64String(hash);
	}

	private static async Task<string> GenerateNewHashForStaticAsync(DatalakeContext db)
	{
		string hash;

		var oldHashList = await db.Users
			.Where(x => !string.IsNullOrEmpty(x.StaticHost))
			.Select(x => x.PasswordHash)
			.ToListAsync();

		int countOfGenerations = 0;
		do
		{
			hash = RandomHash();
			countOfGenerations++;
		}
		while (oldHashList.Any(x => x == hash) && countOfGenerations < 100);

		if (countOfGenerations >= 100)
		{
			throw new DatabaseException(message: "не удалось создать новый уникальный api-ключ за 100 шагов", innerException: null);
		}

		return hash;
	}

	private static string RandomHash()
	{
		using var rng = RandomNumberGenerator.Create();

		var randomNumber = new byte[32];
		rng.GetBytes(randomNumber);

		string refreshToken = Convert.ToBase64String(randomNumber);
		return refreshToken;
	}

	private static async Task LogAsync(
		DatalakeContext db, Guid authorGuid, Guid guid, string message, string? details = null)
	{
		await db.InsertAsync(new Log
		{
			Category = LogCategory.Users,
			RefId = guid.ToString(),
			AffectedUserGuid = guid,
			Text = message,
			Type = LogType.Success,
			AuthorGuid = authorGuid,
			Details = details,
		});
	}

	#endregion

	#region Запросы

	/// <summary>
	/// Список пользователей в БД без удаленных
	/// </summary>
	public static IQueryable<User> UsersNotDeleted(DatalakeContext db)
	{
		return db.Users.Where(x => !x.IsDeleted);
	}

	/// <summary>
	/// Запрос информации о учетных записях
	/// </summary>
	public static IQueryable<UserFlatInfo> GetFlatInfo(DatalakeContext db)
	{
		return db.Users
			.Where(x => !x.IsDeleted)
			.Select(x => new UserFlatInfo
			{
				Guid = x.Guid,
				Login = x.Login,
				FullName = x.FullName ?? string.Empty,
				EnergoIdGuid = x.EnergoIdGuid,
				Type = x.Type,
			});
	}

	/// <summary>
	/// Запрос полной информации о учетных записях, включая группы и права доступа
	/// </summary>
	internal static IQueryable<UserInfo> GetInfo(DatalakeContext db)
	{
		var query =
			from u in db.Users.Where(x => !x.IsDeleted)
			from rel in db.UserGroupRelations.LeftJoin(x => x.UserGuid == u.Guid)
			from g in db.UserGroups.LeftJoin(x => x.Guid == rel.UserGroupGuid && !x.IsDeleted)
			from urights in db.AccessRights.Where(x => x.IsGlobal).LeftJoin(x => x.UserGuid == u.Guid)
			from grights in db.AccessRights.Where(x => x.IsGlobal).LeftJoin(x => x.UserGroupGuid == g.Guid)
			group new { u, g, urights, grights } by u into g
			select new UserInfo
			{
				Login = g.Key.Login,
				Guid = g.Key.Guid,
				Type = g.Key.Type,
				FullName = g.Key.FullName ?? string.Empty,
				EnergoIdGuid = g.Key.EnergoIdGuid,
				UserGroups = g
					.Where(x => x.g != null)
					.Select(x => new UserGroupSimpleInfo
					{
						Guid = x.g.Guid,
						Name = x.g.Name,
					})
					.ToArray(),
				AccessType = (AccessType)g
					.Select(x => Math.Max(
						(int)(x.urights != null ? x.urights.AccessType : AccessType.NotSet),
						(int)(x.urights != null ? x.urights.AccessType : AccessType.NotSet)
					))
					.DefaultIfEmpty((int)AccessType.NoAccess)
					.Max(),
			};

		return query;
	}

	/// <summary>
	/// Получение полной информации о учетных записях, включая группы, права доступа и данные для входа
	/// </summary>
	internal static IQueryable<UserDetailInfo> GetDetailInfo(DatalakeContext db)
	{
		var query =
			from u in db.Users.Where(x => !x.IsDeleted)
			from rel in db.UserGroupRelations.LeftJoin(x => x.UserGuid == u.Guid)
			from g in db.UserGroups.LeftJoin(x => x.Guid == rel.UserGroupGuid && !x.IsDeleted)
			from urights in db.AccessRights.Where(x => x.IsGlobal).LeftJoin(x => x.UserGuid == u.Guid)
			from grights in db.AccessRights.Where(x => x.IsGlobal).LeftJoin(x => x.UserGroupGuid == g.Guid)
			group new { u, g, urights, grights } by u into g
			select new UserDetailInfo
			{
				Login = g.Key.Login,
				Guid = g.Key.Guid,
				Type = g.Key.Type,
				FullName = g.Key.FullName ?? string.Empty,
				EnergoIdGuid = g.Key.EnergoIdGuid,
				UserGroups = g
					.Where(x => x.g != null)
					.Select(x => new UserGroupSimpleInfo
					{
						Guid = x.g.Guid,
						Name = x.g.Name,
					})
					.ToArray(),
				AccessType = (AccessType)g
					.Select(x => Math.Max(
						(int)(x.urights != null ? x.urights.AccessType : AccessType.NotSet),
						(int)(x.urights != null ? x.urights.AccessType : AccessType.NotSet)
					))
					.DefaultIfEmpty((int)AccessType.NoAccess)
					.Max(),
				Hash = g.Key.PasswordHash,
				StaticHost = g.Key.StaticHost,
			};

		return query;
	}

	#endregion
}
