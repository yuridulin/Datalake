using DatalakeApiClasses.Enums;
using DatalakeApiClasses.Exceptions;
using DatalakeApiClasses.Models.Users;
using DatalakeDatabase.Models;
using LinqToDB;
using System.Security.Cryptography;
using System.Text;

namespace DatalakeDatabase.Repositories;

public partial class UsersRepository(DatalakeContext db)
{
	public async Task<UserAuthInfo> AuthenticateAsync(UserLoginPass loginPass)
	{
		var user = await db.Users
			.Where(x => x.Name.ToLower().Trim() == loginPass.Name.ToLower().Trim() && x.StaticHost == null)
			.FirstOrDefaultAsync();

		if (user == null)
		{
			bool haveAnotherUsers = await db.Users
				.Where(x => string.IsNullOrEmpty(x.StaticHost))
				.AnyAsync();

			if (!haveAnotherUsers)
			{
				user = await CreateUserAsync(new UserCreateRequest
				{
					LoginName = loginPass.Name,
					Password = loginPass.Password,
					AccessType = AccessType.ADMIN,
					FullName = loginPass.Name,
					StaticHost = null,
				});

				if (user == null)
				{
					throw new DatabaseException(message: "не удалось создать учётную запись");
				}
			}
			else
			{
				throw new NotFoundException(message: "указанная учётная запись по логину");
			}
		}

		if (!user.Hash.Equals(GetHashFromPassword(loginPass.Password)))
		{
			throw new ForbiddenException(message: "пароль не подходит");
		}

		return new UserAuthInfo
		{
			UserName = loginPass.Name,
			AccessType = user.AccessType,
			Token = string.Empty,
		};
	}

	public async Task<bool> CreateAsync(UserCreateRequest userInfo)
	{
		var user = await CreateUserAsync(userInfo);
		return user != null;
	}

	public async Task<bool> UpdateAsync(string loginName, UserUpdateRequest request)
	{
		if (string.IsNullOrEmpty(request.LoginName))
		{
			throw new InvalidValueException(message: "логин не может быть пустым");
		}

		var userInfo = await GetInfo()
			.Where(x => x.LoginName == loginName)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "пользователь по указанному логину");

		var updateQuery = db.Users
			.Where(x => x.Name == loginName)
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

		if (request.AccessType != AccessType.ADMIN
			&& userInfo.AccessType == AccessType.ADMIN
			&& !await db.Users.Where(x => x.AccessType == AccessType.ADMIN && x.Name != loginName).AnyAsync())
		{
			throw new ForbiddenException(message: "удаление последнего администратора");
		}

		await updateQuery.UpdateAsync();

		return true;
	}

	public async Task<bool> DeleteAsync(string loginName)
	{
		var accessTypeInfo = await db.Users
			.Where(x => x.Name == loginName)
			.Select(x => new { x.AccessType })
			.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "учётная запись по логину");

		if (accessTypeInfo.AccessType == AccessType.ADMIN
			&& !await db.Users.Where(x => x.AccessType == AccessType.ADMIN && x.Name != loginName).AnyAsync())
		{
			throw new ForbiddenException(message: "удаление последнего администратора");
		}

		await db.Users
			.Where(x => x.Name == loginName)
			.DeleteAsync();

		return true;
	}


	private async Task<User> CreateUserAsync(UserCreateRequest userInfo)
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

		await db.Users
			.Value(x => x.Name, user.Name)
			.Value(x => x.FullName, user.FullName)
			.Value(x => x.AccessType, user.AccessType)
			.Value(x => x.Hash, user.Hash)
			.Value(x => x.StaticHost, user.StaticHost)
			.InsertAsync();

		return user;
	}

	private static string GetHashFromPassword(string password)
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

	private static string RandomHash()
	{
		using var rng = RandomNumberGenerator.Create();

		var randomNumber = new byte[32];
		rng.GetBytes(randomNumber);

		string refreshToken = Convert.ToBase64String(randomNumber);
		return refreshToken;
	}
}
