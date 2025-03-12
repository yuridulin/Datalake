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
public class UsersRepository(DatalakeContext db)
{
	#region Действия

	/// <summary>
	/// Создание нового пользователя
	/// </summary>
	/// <param name="user">Идентификатор создающего пользователя</param>
	/// <param name="userInfo">Параметры новой учетной записи</param>
	/// <returns>Идентификатор созданного пользователя</returns>
	public async Task<Guid> CreateAsync(UserAuthInfo user, UserCreateRequest userInfo)
	{
		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Admin);
		User = user.Guid;

		return await CreateAsync(userInfo);
	}

	/// <summary>
	/// Получение информации о пользователях
	/// </summary>
	/// <param name="user">Идентификатор читающего пользователя</param>
	/// <returns>Список пользователей</returns>
	public async Task<UserInfo[]> ReadAllAsync(UserAuthInfo user)
	{
		var users = await db.UsersRepository.GetInfo()
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
	/// <param name="user">Идентификатор читающего пользователя</param>
	/// <param name="guid">Идентификатор затронутого пользователя</param>
	/// <returns>Детальная о пользователе</returns>
	public async Task<UserInfo> ReadAsync(UserAuthInfo user, Guid guid)
	{
		if (user.Guid != guid)
			AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Viewer);

		var userInfo = await db.UsersRepository.GetInfo()
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
	/// <param name="user">Идентификатор читающего пользователя</param>
	/// <param name="guid">Идентификатор затронутого пользователя</param>
	/// <returns>Детальная информация о пользователе</returns>
	public async Task<UserDetailInfo> ReadWithDetailsAsync(UserAuthInfo user, Guid guid)
	{
		if (user.Guid != guid)
			AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Viewer);

		var userInfo = await db.UsersRepository.GetDetailInfo()
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
	/// <param name="user">Идентификатор изменяющего пользователя</param>
	/// <param name="userGuid">Идентификатор затронутого пользователя</param>
	/// <param name="request">Новые параметры учетной записи</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> UpdateAsync(UserAuthInfo user, Guid userGuid, UserUpdateRequest request)
	{
		var accessType = user.Guid == userGuid ? AccessType.Manager : user.GlobalAccessType;

		if (!accessType.HasAccess(AccessType.Manager))
			throw Errors.NoAccess;

		User = user.Guid;

		return await UpdateAsync(userGuid, request);
	}

	/// <summary>
	/// Удаление пользователя
	/// </summary>
	/// <param name="user">Идентификатор удаляющего пользователя</param>
	/// <param name="userGuid">Идентификатор затронутого пользователя</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> DeleteAsync(UserAuthInfo user, Guid userGuid)
	{
		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Admin);
		User = user.Guid;

		return await DeleteAsync(userGuid);
	}

	#endregion

	#region Реализация

	Guid? User { get; set; } = null;

	internal async Task<Guid> CreateAsync(UserCreateRequest request)
	{
		string? hash = null;

		switch (request.Type)
		{
			case UserType.Local:
				if (string.IsNullOrEmpty(request.Login))
					throw new InvalidValueException(message: "логин не может быть пустым");
				if (await db.Users.AnyAsync(x => x.Login == request.Login))
					throw new AlreadyExistException(message: "учётная запись с таким логином");
				if (string.IsNullOrEmpty(request.Password))
					throw new InvalidValueException(message: "необходимо указать пароль");
				hash = GetHashFromPassword(request.Password);
				break;

			case UserType.Static:
				hash = await GenerateNewHashForStaticAsync();
				request.Login = string.Empty;
				break;

			case UserType.EnergoId:
				if (request.EnergoIdGuid == null)
					throw new InvalidValueException(message: "необходимо указать учетную запись EnergoId");
				if (await db.Users.AnyAsync(x => x.EnergoIdGuid == request.EnergoIdGuid))
					throw new AlreadyExistException(message: "учётная запись с выбранным EnergoID");
				hash = string.Empty;
				request.Login = string.Empty;
				break;
		}

		var user = new User
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

		user = await db.Users
			.Value(x => x.Guid, user.Guid)
			.Value(x => x.Login, user.Login)
			.Value(x => x.FullName, user.FullName)
			.Value(x => x.PasswordHash, user.PasswordHash)
			.Value(x => x.StaticHost, user.StaticHost)
			.Value(x => x.EnergoIdGuid, user.EnergoIdGuid)
			.Value(x => x.Type, user.Type)
			.InsertWithOutputAsync();

		await db.AccessRights
			.Value(x => x.UserGuid, user.Guid)
			.Value(x => x.IsGlobal, true)
			.Value(x => x.AccessType, request.AccessType)
			.InsertAsync();

		await LogAsync(user.Guid, "Создан пользователь " + user.FullName);

		await transaction.CommitAsync();

		AccessRepository.Update();

		return user.Guid;
	}

	internal async Task<bool> UpdateAsync(Guid userGuid, UserUpdateRequest request)
	{
		var oldUser = await db.Users
			.Where(x => x.Guid == userGuid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "пользователь по указанному ключу");

		string? newHash = null;
		switch (request.Type)
		{
			case UserType.Local:
				if (string.IsNullOrEmpty(request.Login))
					throw new InvalidValueException(message: "логин не может быть пустым");
				if (await db.Users.AnyAsync(x => x.Login == request.Login && x.Guid != userGuid))
					throw new AlreadyExistException(message: "учётная запись с таким логином");
				if (request.Type != oldUser.Type && string.IsNullOrEmpty(request.Password))
					throw new InvalidValueException(message: "при смене типа учетной записи необходимо задать пароль");
				if (!string.IsNullOrEmpty(request.Password))
					newHash = GetHashFromPassword(request.Password);
				break;

			case UserType.Static:
				request.Login ??= string.Empty;
				if (request.CreateNewStaticHash || request.Type != oldUser.Type)
					newHash = await GenerateNewHashForStaticAsync();
				break;

			case UserType.EnergoId:
				if (request.EnergoIdGuid == null)
					throw new InvalidValueException(message: "необходимо указать учетную запись EnergoId");
				if (await db.Users.AnyAsync(x => x.EnergoIdGuid == request.EnergoIdGuid && x.Guid != userGuid))
					throw new AlreadyExistException(message: "учётная запись с выбранным EnergoID");
				break;
		}

		using var transaction = await db.BeginTransactionAsync();

		int updatedRows = await db.Users
			.Where(x => x.Guid == userGuid)
			.Set(x => x.Type, request.Type)
			.Set(x => x.Login, request.Login)
			.Set(x => x.FullName, request.FullName)
			.Set(x => x.EnergoIdGuid, request.EnergoIdGuid)
			.Set(x => x.PasswordHash, newHash ?? oldUser.PasswordHash)
			.Set(x => x.StaticHost, request.StaticHost ?? oldUser.StaticHost)
			.UpdateAsync();

		updatedRows += await db.AccessRights
			.Where(x => x.UserGuid == userGuid)
			.Set(x => x.AccessType, request.AccessType)
			.UpdateAsync();

		await LogAsync(userGuid, "Изменен пользователь " + (request.FullName ?? request.Login), ObjectExtension.Difference(
			new { oldUser.Type, oldUser.Login, oldUser.FullName, oldUser.EnergoIdGuid, oldUser.StaticHost, Hash = "old" },
			new { request.Type, request.Login, request.FullName, request.EnergoIdGuid, request.StaticHost, Hash = string.IsNullOrEmpty(request.StaticHost) ? "new" : "old" }));

		await transaction.CommitAsync();

		AccessRepository.Update();

		return true;
	}

	internal async Task<bool> DeleteAsync(Guid userGuid)
	{
		using var transaction = await db.BeginTransactionAsync();

		var user = await db.Users
			.Where(x => x.Guid == userGuid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "пользователь " + userGuid);

		await db.AccessRights
			.Where(x => x.UserGuid == userGuid)
			.DeleteAsync();

		await db.UserGroupRelations
			.Where(x => x.UserGuid == userGuid)
			.DeleteAsync();

		await db.Users
			.Where(x => x.Guid == userGuid)
			.DeleteAsync();

		await LogAsync(userGuid, "Удален пользователь " + (user.FullName ?? user.Login));

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

	private async Task<string> GenerateNewHashForStaticAsync()
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

	private async Task LogAsync(Guid guid, string message, string? details = null)
	{
		await db.InsertAsync(new Log
		{
			Category = LogCategory.Users,
			RefId = guid.ToString(),
			Text = message,
			Type = LogType.Success,
			UserGuid = User,
			Details = details,
		});
	}

	#endregion

	#region Запросы

	/// <summary>
	/// Запрос информации о учетных записях
	/// </summary>
	public IQueryable<UserFlatInfo> GetFlatInfo()
	{
		return db.Users
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
	internal IQueryable<UserInfo> GetInfo()
	{
		var query =
			from u in db.Users
			from rel in db.UserGroupRelations.LeftJoin(x => x.UserGuid == u.Guid)
			from g in db.UserGroups.LeftJoin(x => x.Guid == rel.UserGroupGuid)
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
	internal IQueryable<UserDetailInfo> GetDetailInfo()
	{
		var query =
			from u in db.Users
			from rel in db.UserGroupRelations.LeftJoin(x => x.UserGuid == u.Guid)
			from g in db.UserGroups.LeftJoin(x => x.Guid == rel.UserGroupGuid)
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
