using DatalakeApiClasses.Enums;
using DatalakeApiClasses.Exceptions;
using DatalakeApiClasses.Models.Users;
using DatalakeDatabase.Extensions;
using DatalakeDatabase.Models;
using DatalakeDatabase.Repositories.Base;
using LinqToDB;
using System.Security.Cryptography;
using System.Text;

namespace DatalakeDatabase.Repositories;

public partial class UsersRepository(DatalakeContext db) : RepositoryBase
{
	#region Действия

	public async Task<UserAuthInfo> AuthenticateAsync(UserKeycloakInfo info)
	{
		var user = await db.Users
			.Where(x => x.Type == UserType.Keycloak)
			.Where(x => x.KeycloakGuid != null && x.KeycloakGuid == info.KeycloakGuid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "указанная учётная запись по guid");

		return await GetAuthInfo(db, user);
	}

	public async Task<UserAuthInfo> AuthenticateAsync(UserLoginPass loginPass)
	{
		var user = await db.Users
			.Where(x => x.Type == UserType.Local)
			.Where(x => x.Login.ToLower().Trim() == loginPass.Login.ToLower().Trim() && x.StaticHost == null)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "указанная учётная запись по логину");

		if (string.IsNullOrEmpty(user.PasswordHash))
		{
			throw new InvalidValueException(message: "пароль не задан");
		}

		if (!user.PasswordHash.Equals(GetHashFromPassword(loginPass.Password)))
		{
			throw new ForbiddenException(message: "пароль не подходит");
		}

		return await GetAuthInfo(db, user);
	}

	public async Task<Guid> CreateAsync(UserAuthInfo user, UserCreateRequest userInfo)
	{
		CheckGlobalAccess(user, AccessType.Admin);

		return await CreateAsync(userInfo);
	}

	public async Task<bool> UpdateAsync(UserAuthInfo user, Guid userGuid, UserUpdateRequest request)
	{
		CheckGlobalAccess(user, AccessType.Admin);

		return await UpdateAsync(userGuid, request);
	}

	public async Task<bool> DeleteAsync(UserAuthInfo user, Guid userGuid)
	{
		CheckGlobalAccess(user, AccessType.Admin);

		return await DeleteAsync(userGuid);
	}

	#endregion


	#region Реализация

	internal async Task<Guid> CreateAsync(UserCreateRequest userInfo)
	{
		string hash;

		if (string.IsNullOrEmpty(userInfo.Login))
		{
			throw new InvalidValueException(message: "логин не может быть пустым");
		}

		if (!string.IsNullOrEmpty(userInfo.StaticHost))
		{
			hash = await GenerateNewHashForStaticAsync();
		}
		else if (!string.IsNullOrEmpty(userInfo.Password))
		{
			hash = GetHashFromPassword(userInfo.Password);
		}
		else
		{
			throw new InvalidValueException(message: "для учётной записи обязательно наличие или пароля, или адреса сторонней службы");
		}

		if (await db.Users.AnyAsync(x => x.Login == userInfo.Login))
		{
			throw new AlreadyExistException(message: "учётная запись с таким логином");
		}

		var user = new User
		{
			Guid = Guid.NewGuid(),
			Login = userInfo.Login,
			FullName = userInfo.FullName ?? userInfo.Login,
			PasswordHash = hash,
			StaticHost = userInfo.StaticHost,
		};

		using var transaction = await db.BeginTransactionAsync();

		user = await db.Users
			.Value(x => x.Guid, user.Guid)
			.Value(x => x.Login, user.Login)
			.Value(x => x.FullName, user.FullName)
			.Value(x => x.PasswordHash, user.PasswordHash)
			.Value(x => x.StaticHost, user.StaticHost)
			.InsertWithOutputAsync();

		await db.AccessRights
			.Value(x => x.UserGuid, user.Guid)
			.Value(x => x.IsGlobal, true)
			.Value(x => x.AccessType, userInfo.AccessType)
			.InsertAsync();

		await transaction.CommitAsync();

		return user.Guid;
	}

	internal async Task<bool> UpdateAsync(Guid userGuid, UserUpdateRequest request)
	{
		if (string.IsNullOrEmpty(request.Login))
		{
			throw new InvalidValueException(message: "логин не может быть пустым");
		}

		var userInfo = await GetInfo()
			.Where(x => x.Guid == userGuid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "пользователь по указанному логину");

		var updateQuery = db.Users
			.Where(x => x.Guid == userGuid)
			.Set(x => x.Login, request.Login)
			.Set(x => x.FullName, request.FullName);

		bool newHashWasGenerated = false;

		if (!string.IsNullOrEmpty(request.Login))
		{
			updateQuery
				.Set(x => x.Login, request.Login);
		}

		if (!string.IsNullOrEmpty(request.StaticHost))
		{
			string hash = await GenerateNewHashForStaticAsync();

			updateQuery
				.Set(x => x.StaticHost, request.StaticHost)
				.Set(x => x.PasswordHash, hash);

			newHashWasGenerated = true;
		}

		if (request.CreateNewStaticHash)
		{
			if (string.IsNullOrEmpty(request.StaticHost))
			{
				throw new InvalidValueException(message: "нельзя сгенерировать новый api-ключ для нестатической учётной записи");
			}

			if (!newHashWasGenerated)
			{
				string hash = await GenerateNewHashForStaticAsync();

				updateQuery
					.Set(x => x.PasswordHash, hash);
			}
		}

		if (!string.IsNullOrEmpty(request.Password))
		{
			string hash = GetHashFromPassword(request.Password);

			updateQuery
				.Set(x => x.StaticHost, null as string)
				.Set(x => x.PasswordHash, hash);
		}

		await db.AccessRights
			.Where(x => x.UserGuid == userGuid)
			.Set(x => x.AccessType, request.AccessType)
			.UpdateAsync();

		await updateQuery.UpdateAsync();

		return true;
	}

	internal async Task<bool> DeleteAsync(Guid userGuid)
	{
		using var transaction = await db.BeginTransactionAsync();

		await db.AccessRights
			.Where(x => x.UserGuid == userGuid)
			.DeleteAsync();

		await db.UserGroupRelations
			.Where(x => x.UserGuid == userGuid)
			.DeleteAsync();

		await db.Users
			.Where(x => x.Guid == userGuid)
			.DeleteAsync();

		await transaction.CommitAsync();

		return true;
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
			throw new DatabaseException(message: "не удалось создать новый уникальный api-ключ за 100 шагов");
		}

		return hash;
	}

	private static string GetHashFromPassword(string password)
	{
		if (string.IsNullOrEmpty(password))
			throw new InvalidValueException(message: "пароль не может быть пустым");

		var hash = SHA1.HashData(Encoding.UTF8.GetBytes(password));
		return Convert.ToBase64String(hash);
	}

	private static string RandomHash()
	{
		using var rng = RandomNumberGenerator.Create();

		var randomNumber = new byte[32];
		rng.GetBytes(randomNumber);

		string refreshToken = Convert.ToBase64String(randomNumber);
		return refreshToken;
	}

	private static async Task<UserAuthInfo> GetAuthInfo(DatalakeContext db, User user)
	{
		var accessRights = await db.AuthorizeUserAsync(user.Guid);
		var globalAccessType = accessRights
			.Where(x => x.IsGlobal)
			.Select(x => (int)x.AccessType)
			.DefaultIfEmpty((int)AccessType.NoAccess)
			.Max();

		return new UserAuthInfo
		{
			Guid = user.Guid,
			Login = user.Login,
			Token = string.Empty,
			GlobalAccessType = (AccessType)globalAccessType,
			Rights = accessRights
				.Select(x => new UserAccessRightsInfo
				{
					AccessType = x.AccessType,
					IsGlobal = x.IsGlobal,
					BlockId = x.BlockId,
					SourceId = x.SourceId,
					TagId = x.TagId,
				})
				.ToArray(),
		};
	}

	#endregion
}
