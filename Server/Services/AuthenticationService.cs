using Datalake.Database.Functions;
using Datalake.Database.InMemory;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Users;

namespace Datalake.Server.Services;

/// <summary>
/// Сервис аутентификации пользователей по входным данным
/// </summary>
/// <param name="dataStore"></param>
/// <param name="derivedDataStore"></param>
public class AuthenticationService(
	DatalakeDataStore dataStore,
	DatalakeDerivedDataStore derivedDataStore)
{
	/// <summary>
	/// Получение информации о пользователе по данным из EnergoId
	/// </summary>
	/// <param name="info">Данные о учетной записи</param>
	/// <returns>Информация о пользователе, включая доступ</returns>
	public UserAuthInfo Authenticate(UserEnergoIdInfo info)
	{
		return derivedDataStore.Access.Get(info.EnergoIdGuid);
	}

	/// <summary>
	/// Получение информации о пользователе по логину и паролю
	/// </summary>
	/// <param name="loginPass">Логин и пароль</param>
	/// <returns>Информация о пользователе, включая доступ</returns>
	public UserAuthInfo Authenticate(UserLoginPass loginPass)
	{
		var user = dataStore.State.Users
			.Where(x => x.Type == UserType.Local)
			.Where(x => x.Login != null && x.Login.ToLower().Trim() == loginPass.Login.ToLower().Trim())
			.FirstOrDefault()
			?? throw new NotFoundException(message: "указанная учётная запись по логину");

		if (string.IsNullOrEmpty(user.PasswordHash))
		{
			throw new InvalidValueException(message: "пароль не задан");
		}

		if (!user.PasswordHash.Equals(Passwords.GetHashFromPassword(loginPass.Password)))
		{
			throw new ForbiddenException(message: "пароль не подходит");
		}

		return derivedDataStore.Access.Get(user.Guid);
	}
}
