using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Exceptions;
using Datalake.ApiClasses.Models.Users;
using Datalake.Database.Extensions;
using Datalake.Database.Models;
using LinqToDB;
using System.Security.Cryptography;
using System.Text;

namespace Datalake.Database.Repositories;

public partial class UsersRepository(DatalakeContext db)
{
	#region Действия

	public async Task<Guid> CreateAsync(UserAuthInfo user, UserCreateRequest userInfo)
	{
		await db.AccessRepository.CheckGlobalAccess(user, AccessType.Admin);
		User = user.Guid;

		return await CreateAsync(userInfo);
	}

	public async Task<bool> UpdateAsync(UserAuthInfo user, Guid userGuid, UserUpdateRequest request)
	{
		await db.AccessRepository.CheckGlobalAccess(user, AccessType.Admin);
		User = user.Guid;

		return await UpdateAsync(userGuid, request);
	}

	public async Task<bool> DeleteAsync(UserAuthInfo user, Guid userGuid)
	{
		await db.AccessRepository.CheckGlobalAccess(user, AccessType.Admin);
		User = user.Guid;

		return await DeleteAsync(userGuid);
	}

	#endregion


	#region Реализация

	Guid User { get; set; }

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
}
