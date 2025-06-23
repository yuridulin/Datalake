using Datalake.Database.Constants;
using Datalake.Database.Extensions;
using Datalake.Database.InMemory.Models;
using Datalake.Database.Repositories;
using Datalake.Database.Tables;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Users;
using LinqToDB;
using System.Security.Cryptography;
using System.Text;

namespace Datalake.Database.InMemory.Repositories;

/// <summary>
/// Репозиторий работы с пользователями в памяти приложения
/// </summary>
public class UsersMemoryRepository(DatalakeDataStore dataStore)
{
	#region Действия

	/// <summary>
	/// Создание нового пользователя
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Идентификатор создающего пользователя</param>
	/// <param name="userInfo">Параметры новой учетной записи</param>
	/// <returns>Идентификатор созданного пользователя</returns>
	public async Task<UserInfo> CreateAsync(
		DatalakeContext db, UserAuthInfo user, UserCreateRequest userInfo)
	{
		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		return await ProtectedCreateAsync(db, user.Guid, userInfo);
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
		DatalakeContext db, UserAuthInfo user, Guid userGuid, UserUpdateRequest request)
	{
		var accessType = user.Guid == userGuid ? AccessType.Manager : user.GlobalAccessType;

		if (!accessType.HasAccess(AccessType.Manager))
			throw Errors.NoAccess;

		return await ProtectedUpdateAsync(db, user.Guid, userGuid, request);
	}

	/// <summary>
	/// Удаление пользователя
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Идентификатор удаляющего пользователя</param>
	/// <param name="userGuid">Идентификатор затронутого пользователя</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> DeleteAsync(
		DatalakeContext db, UserAuthInfo user, Guid userGuid)
	{
		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		return await ProtectedDeleteAsync(db, user.Guid, userGuid);
	}

	#endregion

	internal async Task<UserInfo> ProtectedCreateAsync(
		DatalakeContext db, Guid userGuid, UserCreateRequest request)
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
				hash = GetHashFromPassword(request.Password);
				break;

			case UserType.Static:
				hash = await GenerateNewHashForStaticAsync(db);
				request.Login = string.Empty;
				break;

			case UserType.EnergoId:
				if (request.EnergoIdGuid == null)
					throw new InvalidValueException(message: "необходимо указать учетную запись EnergoId");
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

		AccessRights createdGlobalRule = new()
		{
			UserGuid = createdUser.Guid,
			IsGlobal = true,
			AccessType = request.AccessType,
		};

		// Блокируем стейт до завершения обновления
		DatalakeDataState currentState;
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
				throw new Exception("Не удалось создать тег в БД", ex);
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
		DatalakeContext db, Guid userGuid, Guid affectedUserGuid, UserUpdateRequest request)
	{
		// Проверки, не требующие стейта
		string? newHash = null;

		switch (request.Type)
		{
			case UserType.Local:
				if (string.IsNullOrEmpty(request.Login))
					throw new InvalidValueException(message: "логин не может быть пустым");
				if (!string.IsNullOrEmpty(request.Password))
					newHash = GetHashFromPassword(request.Password);
				break;

			case UserType.Static:
				request.Login ??= string.Empty;
				break;

			case UserType.EnergoId:
				if (request.EnergoIdGuid == null)
					throw new InvalidValueException(message: "необходимо указать учетную запись EnergoId");
				break;
		}

		User updatedUser;
		AccessRights? updatedGlobalRule = null;

		// Блокируем стейт до завершения обновления
		DatalakeDataState currentState;
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
						newHash = await GenerateNewHashForStaticAsync(db);
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
						.Where(x => x.UserGuid == affectedUserGuid)
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
				throw new Exception("Не удалось создать тег в БД", ex);
			}

			// Обновление стейта в случае успешного обновления БД
			dataStore.UpdateStateWithinLock(state => state with
			{
				Users = state.Users.Remove(oldUser).Add(updatedUser),
				AccessRights = (updatedGlobalRule != null && globalRule != null)
					? state.AccessRights.Remove(globalRule).Add(updatedGlobalRule)
					: state.AccessRights,
			});
		}

		// Возвращение ответа
		return true;
	}

	internal async Task<bool> ProtectedDeleteAsync(
		DatalakeContext db, Guid userGuid, Guid affectedUserGuid)
	{
		// Проверки, не требующие стейта
		User updatedUser;

		// Блокируем стейт до завершения обновления
		DatalakeDataState currentState;
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
				throw new Exception("Не удалось создать тег в БД", ex);
			}

			// Обновление стейта в случае успешного обновления БД
			dataStore.UpdateStateWithinLock(state => state with
			{
				Users = state.Users.Remove(oldUser).Add(updatedUser),
			});
		}

		// Возвращение ответа

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
			AuthorGuid = authorGuid == Guid.Empty ? null : authorGuid,
			Details = details,
		});
	}
}
