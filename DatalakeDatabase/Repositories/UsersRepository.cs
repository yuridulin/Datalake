using DatalakeApiClasses.Enums;
using DatalakeApiClasses.Exceptions;
using DatalakeApiClasses.Models.Users;
using DatalakeDatabase.Extensions;
using DatalakeDatabase.Models;
using LinqToDB;
using System.Security.Cryptography;
using System.Text;

namespace DatalakeDatabase.Repositories;

public partial class UsersRepository(DatalakeContext db)
{
	#region Действия

	public async Task<UserAuthInfo> AuthenticateAsync(UserLoginPass loginPass)
	{
		var user = await db.Users
			.Where(x => x.Name.ToLower().Trim() == loginPass.Name.ToLower().Trim() && x.StaticHost == null)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "указанная учётная запись по логину");

		if (!user.Hash.Equals(GetHashFromPassword(loginPass.Password)))
		{
			throw new ForbiddenException(message: "пароль не подходит");
		}

		var accessRights = await db.AuthorizeUserAsync(user.UserGuid);
		var globalAccessType = accessRights
			.Where(x => x.IsGlobal)
			.Select(x => (int)x.AccessType)
			.DefaultIfEmpty((int)AccessType.NoAccess)
			.Max();

		return new UserAuthInfo
		{
			UserGuid = user.UserGuid,
			UserName = loginPass.Name,
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

	public async Task<Guid> CreateAsync(UserAuthInfo user, UserCreateRequest userInfo)
	{
		await db.CheckAccessAsync(user, AccessType.Admin, AccessScope.Global);

		return await CreateAsync(userInfo);
	}

	public async Task<bool> UpdateAsync(UserAuthInfo user, Guid userGuid, UserUpdateRequest request)
	{
		await db.CheckAccessAsync(user, AccessType.Admin, AccessScope.Global);

		return await UpdateAsync(userGuid, request);
	}

	public async Task<bool> DeleteAsync(UserAuthInfo user, Guid userGuid)
	{
		await db.CheckAccessAsync(user, AccessType.Admin, AccessScope.Global);

		return await DeleteAsync(userGuid);
	}

	#endregion

	#region Реализация

	internal async Task<Guid> CreateAsync(UserCreateRequest userInfo)
	{
		string hash;

		if (string.IsNullOrEmpty(userInfo.LoginName))
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

		if (await db.Users.AnyAsync(x => x.Name == userInfo.LoginName))
		{
			throw new AlreadyExistException(message: "учётная запись с таким логином");
		}

		var user = new User
		{
			UserGuid = Guid.NewGuid(),
			Name = userInfo.LoginName,
			FullName = userInfo.FullName ?? userInfo.LoginName,
			AccessType = userInfo.AccessType,
			Hash = hash,
			StaticHost = userInfo.StaticHost,
		};

		using var transaction = await db.BeginTransactionAsync();

		user = await db.Users
			.Value(x => x.UserGuid, user.UserGuid)
			.Value(x => x.Name, user.Name)
			.Value(x => x.FullName, user.FullName)
			.Value(x => x.AccessType, user.AccessType)
			.Value(x => x.Hash, user.Hash)
			.Value(x => x.StaticHost, user.StaticHost)
			.InsertWithOutputAsync();

		await db.AccessRights
			.Value(x => x.UserGuid, user.UserGuid)
			.Value(x => x.IsGlobal, true)
			.Value(x => x.AccessType, user.AccessType)
			.InsertAsync();

		await transaction.CommitAsync();

		return user.UserGuid;
	}

	internal async Task<bool> UpdateAsync(Guid userGuid, UserUpdateRequest request)
	{
		if (string.IsNullOrEmpty(request.LoginName))
		{
			throw new InvalidValueException(message: "логин не может быть пустым");
		}

		var userInfo = await GetInfo()
			.Where(x => x.UserGuid == userGuid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "пользователь по указанному логину");

		var updateQuery = db.Users
			.Where(x => x.UserGuid == userGuid)
			.Set(x => x.Name, request.LoginName)
			.Set(x => x.FullName, request.FullName)
			.Set(x => x.AccessType, request.AccessType);

		bool newHashWasGenerated = false;

		if (!string.IsNullOrEmpty(request.LoginName))
		{
			updateQuery
				.Set(x => x.Name, request.LoginName);
		}

		if (!string.IsNullOrEmpty(request.StaticHost))
		{
			string hash = await GenerateNewHashForStaticAsync();

			updateQuery
				.Set(x => x.StaticHost, request.StaticHost)
				.Set(x => x.Hash, hash);

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
					.Set(x => x.Hash, hash);
			}
		}

		if (!string.IsNullOrEmpty(request.Password))
		{
			string hash = GetHashFromPassword(request.Password);

			updateQuery
				.Set(x => x.StaticHost, null as string)
				.Set(x => x.Hash, hash);
		}

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
			.Where(x => x.UserGuid == userGuid)
			.DeleteAsync();

		await transaction.CommitAsync();

		return true;
	}

	private async Task<string> GenerateNewHashForStaticAsync()
	{
		string hash;

		var oldHashList = await db.Users
				.Where(x => !string.IsNullOrEmpty(x.StaticHost))
				.Select(x => x.Hash)
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

	#endregion
}
