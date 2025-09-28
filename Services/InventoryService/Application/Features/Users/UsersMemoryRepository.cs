using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Domain.Extensions;
using Datalake.InventoryService.Domain.Services;
using Datalake.InventoryService.Infrastructure.Cache.EnergoId;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Cache.Inventory.Services;
using Datalake.InventoryService.Infrastructure.Database;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Users;

namespace Datalake.InventoryService.Application.Features.Users;

/// <summary>
/// Репозиторий работы с пользователями в памяти приложения
/// </summary>
public class UsersMemoryRepository(
	InventoryCacheStore dataStore,
	EnergoIdCacheStore energoIdStore)
{
	#region API

	/// <summary>
	/// Создание нового пользователя
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Идентификатор создающего пользователя</param>
	/// <param name="userInfo">Параметры новой учетной записи</param>
	/// <returns>Идентификатор созданного пользователя</returns>
	public async Task<UserInfo> CreateAsync(
		InventoryEfContext db, UserAuthInfo user, UserCreateRequest userInfo)
	{
		user.ThrowIfNoGlobalAccess(AccessType.Admin);

		return await ProtectedCreateAsync(db, user.Guid, userInfo);
	}

	/// <summary>
	/// Получение информации о пользователях
	/// </summary>
	/// <param name="user">Идентификатор читающего пользователя</param>
	/// <returns>Список пользователей</returns>
	public UserInfo[] GetAll(UserAuthInfo user)
	{
		var users = dataStore.State.UsersInfo();
		var energoId = energoIdStore.State;

		List<UserInfo> usersWithAccess = [];
		foreach (var u in users)
		{
			u.AccessRule = (user.Guid == u.Guid && !user.RootRule.HasAccess(AccessType.Manager))
				? new(0, AccessType.Manager)
				: new(0, user.RootRule.Access);

			if (!u.AccessRule.HasAccess(AccessType.Manager))
			{
				u.FullName = string.Empty;
				u.AccessType = AccessType.NotSet;
				u.Login = string.Empty;
				u.Type = UserType.Local;
				u.UserGroups = [];
				u.Guid = Guid.Empty;
			}

			if (!u.AccessRule.HasAccess(AccessType.Viewer))
				continue;

			usersWithAccess.Add(u);

			if (u.Type == UserType.EnergoId && u.EnergoIdGuid.HasValue && energoId.UsersByGuid.TryGetValue(u.EnergoIdGuid.Value, out var energo))
				u.FullName = energo.GetFullName;
		}

		return usersWithAccess.ToArray();
	}

	/// <summary>
	/// Получение списка пользователей EnergoId с сопоставлением с существующими пользователями
	/// </summary>
	/// <param name="user">Идентификатор читающего пользователя</param>
	/// <returns></returns>
	public UserEnergoIdInfo[] GetEnergoId(UserAuthInfo user)
	{
		user.ThrowIfNoGlobalAccess(AccessType.Admin);

		var currentMapped = dataStore.State.Users
			.Where(x => !x.IsDeleted && x.EnergoIdGuid != null)
			.ToDictionary(x => x.EnergoIdGuid!.Value, x => x.Guid);

		var all = energoIdStore.State.Users
			.Select(x => new UserEnergoIdInfo
			{
				EnergoIdGuid = x.Guid,
				FullName = x.GetFullName,
				Email = x.Email ?? "нет адреса",
				UserGuid = currentMapped.TryGetValue(x.Guid, out var userGuid) ? userGuid : null,
			})
			.ToArray();

		return all;
	}

	/// <summary>
	/// Получение информации о пользователе
	/// </summary>
	/// <param name="user">Идентификатор читающего пользователя</param>
	/// <param name="guid">Идентификатор затронутого пользователя</param>
	/// <returns>Детальная о пользователе</returns>
	public UserInfo Get(UserAuthInfo user, Guid guid)
	{
		if (user.Guid != guid)
			user.ThrowIfNoGlobalAccess(AccessType.Viewer);

		var userInfo = dataStore.State.UsersInfo().FirstOrDefault(x => x.Guid == guid)
			?? throw new NotFoundException($"Учётная запись {guid}");

		foreach (var group in userInfo.UserGroups)
			group.AccessRule = user.GetAccessToUserGroup(group.Guid);

		userInfo.AccessRule = (user.Guid == guid && !user.RootRule.Access.HasAccess(AccessType.Manager))
			? new(0, AccessType.Manager)
			: new(0, user.RootRule.Access);

		if (userInfo.Type == UserType.EnergoId
			&& userInfo.EnergoIdGuid.HasValue
			&& energoIdStore.State.UsersByGuid.TryGetValue(userInfo.EnergoIdGuid.Value, out var energo))
		{
			userInfo.FullName = energo.GetFullName;
		}

		return userInfo;
	}

	/// <summary>
	/// Получение детальной информации о пользователе, включая группы и правила
	/// </summary>
	/// <param name="user">Идентификатор читающего пользователя</param>
	/// <param name="guid">Идентификатор затронутого пользователя</param>
	/// <returns>Детальная информация о пользователе</returns>
	public UserDetailInfo GetWithDetails(UserAuthInfo user, Guid guid)
	{
		if (user.Guid != guid)
			user.ThrowIfNoGlobalAccess(AccessType.Viewer);

		var userInfo = dataStore.State.UsersDetailInfo().FirstOrDefault(x => x.Guid == guid)
			?? throw new NotFoundException($"Учётная запись {guid}");

		userInfo.AccessRule = (user.Guid == guid && !user.RootRule.Access.HasAccess(AccessType.Manager))
			? new(0, AccessType.Manager)
			: new(0, user.RootRule.Access);

		foreach (var group in userInfo.UserGroups)
			group.AccessRule = user.GetAccessToUserGroup(group.Guid);

		if (userInfo.Type == UserType.EnergoId
			&& userInfo.EnergoIdGuid.HasValue
			&& energoIdStore.State.UsersByGuid.TryGetValue(userInfo.EnergoIdGuid.Value, out var energo))
		{
			userInfo.FullName = energo.GetFullName;
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
	public async Task<bool> UpdateAsync(
		InventoryEfContext db, UserAuthInfo user, Guid userGuid, UserUpdateRequest request)
	{
		var accessType = user.Guid == userGuid ? AccessType.Manager : user.RootRule.Access;

		if (!accessType.HasAccess(AccessType.Manager))
			throw Errors.NoAccess;

		return await ProtectedUpdateAsync(db, user.Guid, userGuid, request);
	}

	/// <summary>
	/// Обновление информации из EnergoId
	/// </summary>
	/// <param name="user">Идентификатор изменяющего пользователя</param>
	public void UpdateEnergoId(UserAuthInfo user)
	{
		user.ThrowIfNoGlobalAccess(AccessType.Admin);

		energoIdStore.UpdateAsync();
	}

	/// <summary>
	/// Удаление пользователя
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Идентификатор удаляющего пользователя</param>
	/// <param name="userGuid">Идентификатор затронутого пользователя</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> DeleteAsync(
		InventoryEfContext db, UserAuthInfo user, Guid userGuid)
	{
		user.ThrowIfNoGlobalAccess(AccessType.Admin);

		return await ProtectedDeleteAsync(db, user.Guid, userGuid);
	}

	#endregion API

	#region Действия

	internal async Task<UserInfo> ProtectedCreateAsync(
		InventoryEfContext db, Guid userGuid, UserCreateRequest request)
	{
		// Проверки, не требующие стейта
		string? hash = null;

		switch (request.Type)
		{
			case UserType.Local:
				if (string.IsNullOrEmpty(request.Login))
					throw new InvalidValueException(message: "логин не может быть пустым");
				if (string.IsNullOrEmpty(request.Password))
					throw new InvalidValueException(message: "необходимо указать пароль");
				hash = PasswordHash.GetHashFromPassword(request.Password);
				break;

			case UserType.Static:
				hash = await CreateNewStaticHash(db, request.StaticHost!);
				request.Login = string.Empty;
				break;

			case UserType.EnergoId:
				if (request.EnergoIdGuid == null)
					throw new InvalidValueException(message: "необходимо указать учетную запись EnergoId");
				hash = string.Empty;
				request.Login = string.Empty;
				break;
		}

		var createdUser = new UserEntity
		{
			Guid = Guid.NewGuid(),
			Login = request.Login,
			FullName = request.FullName ?? request.Login,
			PasswordHash = hash,
			StaticHost = request.StaticHost,
			EnergoIdGuid = request.EnergoIdGuid,
			Type = request.Type,
		};

		AccessRuleEntity createdGlobalRule = new()
		{
			UserGuid = createdUser.Guid,
			IsGlobal = true,
			AccessType = request.AccessType,
		};

		// Блокируем стейт до завершения обновления
		InventoryState currentState;
		using (await dataStore.AcquireWriteLockAsync())
		{
			currentState = dataStore.State;

			// Проверки на актуальном стейте
			switch (request.Type)
			{
				case UserType.Local:
					if (currentState.Users.Any(x => !x.IsDeleted && x.Login == request.Login))
						throw new AlreadyExistException(message: "учётная запись с таким логином");
					break;

				case UserType.EnergoId:
					if (currentState.Users.Any(x => !x.IsDeleted && x.EnergoIdGuid == request.EnergoIdGuid))
						throw new AlreadyExistException(message: "учётная запись с выбранным EnergoID");
					break;
			}

			// Обновление в БД
			using var transaction = await db.BeginTransactionAsync();
			try
			{
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
					.Value(x => x.UserGuid, createdGlobalRule.UserGuid)
					.Value(x => x.IsGlobal, createdGlobalRule.IsGlobal)
					.Value(x => x.AccessType, createdGlobalRule.AccessType)
					.InsertAsync();

				await LogAsync(db, userGuid, createdUser.Guid, "Создан пользователь " + createdUser.FullName);

				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception("Не удалось создать пользователя в БД", ex);
			}

			// Обновление стейта в случае успешного обновления БД
			dataStore.UpdateStateWithinLock(state => state with
			{
				Users = state.Users.Add(createdUser),
				AccessRights = state.AccessRights.Add(createdGlobalRule),
			});
		}

		// Возвращение ответа
		var info = new UserInfo
		{
			AccessType = createdGlobalRule.AccessType,
			Guid = createdUser.Guid,
			FullName = createdUser.FullName ?? string.Empty,
			EnergoIdGuid = createdUser.EnergoIdGuid,
			Login = createdUser.Login,
			Type = createdUser.Type,
		};

		return info;
	}

	internal async Task<bool> ProtectedUpdateAsync(
		InventoryEfContext db, Guid userGuid, Guid affectedUserGuid, UserUpdateRequest request)
	{
		// Проверки, не требующие стейта
		string? newHash = null;

		switch (request.Type)
		{
			case UserType.Local:
				if (string.IsNullOrEmpty(request.Login))
					throw new InvalidValueException(message: "логин не может быть пустым");
				if (!string.IsNullOrEmpty(request.Password))
					newHash = PasswordHash.GetHashFromPassword(request.Password);
				break;

			case UserType.Static:
				request.Login ??= string.Empty;
				break;

			case UserType.EnergoId:
				if (request.EnergoIdGuid == null)
					throw new InvalidValueException(message: "необходимо указать учетную запись EnergoId");
				break;
		}

		UserEntity updatedUser;
		AccessRuleEntity? updatedGlobalRule = null;

		// Блокируем стейт до завершения обновления
		InventoryState currentState;
		using (await dataStore.AcquireWriteLockAsync())
		{
			currentState = dataStore.State;

			// Проверки на актуальном стейте
			if (!currentState.UsersByGuid.TryGetValue(affectedUserGuid, out var oldUser))
				throw new NotFoundException(message: "пользователь по указанному ключу");

			var globalRule = currentState.AccessRights.FirstOrDefault(x => x.UserGuid == affectedUserGuid && x.IsGlobal);
			if (globalRule != null && globalRule.AccessType != request.AccessType)
				updatedGlobalRule = globalRule with { AccessType = request.AccessType };

			switch (request.Type)
			{
				case UserType.Local:
					if (currentState.Users.Any(x => !x.IsDeleted && x.Login == request.Login && x.Guid != affectedUserGuid))
						throw new AlreadyExistException(message: "учётная запись с таким логином");
					if (request.Type != oldUser.Type && string.IsNullOrEmpty(request.Password))
						throw new InvalidValueException(message: "при смене типа учетной записи необходимо задать пароль");
					break;

				case UserType.Static:
					if (request.CreateNewStaticHash || request.Type != oldUser.Type)
						newHash = await CreateNewStaticHash(db, request.StaticHost!);
					break;

				case UserType.EnergoId:
					if (currentState.Users.Any(x => !x.IsDeleted && x.EnergoIdGuid == request.EnergoIdGuid && x.Guid != affectedUserGuid))
						throw new AlreadyExistException(message: "учётная запись с выбранным EnergoID");
					break;
			}

			updatedUser = oldUser with
			{
				Type = request.Type,
				Login = request.Login,
				FullName = request.FullName,
				EnergoIdGuid = request.EnergoIdGuid,
				PasswordHash = newHash ?? oldUser.PasswordHash,
				StaticHost = request.StaticHost ?? oldUser.StaticHost,
			};

			// Обновление в БД
			using var transaction = await db.BeginTransactionAsync();

			try
			{
				int updatedRows = await db.Users
					.Where(x => x.Guid == affectedUserGuid)
					.Set(x => x.Type, updatedUser.Type)
					.Set(x => x.Login, updatedUser.Login)
					.Set(x => x.FullName, updatedUser.FullName)
					.Set(x => x.EnergoIdGuid, updatedUser.EnergoIdGuid)
					.Set(x => x.PasswordHash, updatedUser.PasswordHash)
					.Set(x => x.StaticHost, updatedUser.StaticHost)
					.UpdateAsync();

				if (updatedGlobalRule != null)
				{
					updatedRows += await db.AccessRights
						.Where(x => x.Id == updatedGlobalRule.Id)
						.Set(x => x.AccessType, updatedGlobalRule.AccessType)
						.UpdateAsync();
				}

				await LogAsync(db, userGuid, affectedUserGuid, "Изменен пользователь " + (request.FullName ?? request.Login), ObjectExtension.Difference(
					new { oldUser.Type, oldUser.Login, oldUser.FullName, oldUser.EnergoIdGuid, oldUser.StaticHost, Hash = "old" },
					new { request.Type, request.Login, request.FullName, request.EnergoIdGuid, request.StaticHost, Hash = string.IsNullOrEmpty(request.StaticHost) ? "new" : "old" }));

				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception("Не удалось обновить пользователя в БД", ex);
			}

			// Обновление стейта в случае успешного обновления БД
			dataStore.UpdateStateWithinLock(state => state with
			{
				Users = state.Users.Replace(oldUser, updatedUser),
				AccessRights = (updatedGlobalRule != null && globalRule != null)
					? state.AccessRights.Replace(globalRule, updatedGlobalRule)
					: state.AccessRights,
			});
		}

		// Возвращение ответа
		return true;
	}

	internal async Task<bool> ProtectedDeleteAsync(
		InventoryEfContext db, Guid userGuid, Guid affectedUserGuid)
	{
		// Проверки, не требующие стейта
		UserEntity updatedUser;

		// Блокируем стейт до завершения обновления
		InventoryState currentState;
		using (await dataStore.AcquireWriteLockAsync())
		{
			currentState = dataStore.State;

			// Проверки на актуальном стейте
			if (!currentState.UsersByGuid.TryGetValue(affectedUserGuid, out var oldUser))
				throw new NotFoundException(message: "пользователь " + affectedUserGuid);

			updatedUser = oldUser with { IsDeleted = true };

			// Обновление в БД
			using var transaction = await db.BeginTransactionAsync();

			try
			{
				await db.Users
					.Where(x => x.Guid == affectedUserGuid)
					.Set(x => x.IsDeleted, updatedUser.IsDeleted)
					.UpdateAsync();

				await LogAsync(db, userGuid, affectedUserGuid, "Удален пользователь " + (oldUser.FullName ?? oldUser.Login));

				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception("Не удалось удалить пользователя", ex);
			}

			// Обновление стейта в случае успешного обновления БД
			dataStore.UpdateStateWithinLock(state => state with
			{
				Users = state.Users.Replace(oldUser, updatedUser),
			});
		}

		// Возвращение ответа

		return true;
	}

	private static async Task LogAsync(
		InventoryEfContext db, Guid authorGuid, Guid guid, string message, string? details = null)
	{
		await db.InsertAsync(new Audit
		{
			Category = LogCategory.Users,
			RefId = guid.ToString(),
			AffectedUserGuid = guid,
			Text = message,
			Type = LogType.Success,
			AuthorGuid = authorGuid == Guid.Empty ? null : authorGuid,
			Details = details,
		});
	}

	private static async Task<string?> CreateNewStaticHash(InventoryEfContext db, string host)
	{
		var oldHashes = await db.Users
			.Where(user => user.Type == UserType.Static && user.StaticHost == host)
			.Where(x => !string.IsNullOrEmpty(x.PasswordHash))
			.Select(x => x.PasswordHash!)
			.ToArrayAsync();

		return PasswordHash.GenerateNewHashForStatic(oldHashes.ToHashSet());
	}

	#endregion Действия
}