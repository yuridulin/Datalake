using Datalake.Database;
using Datalake.Database.InMemory.Repositories;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Users;
using Datalake.Server.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Server.Controllers;

/// <summary>
/// Взаимодействие с пользователями
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class UsersController(
	DatalakeContext db,
	AuthenticationService authenticator,
	AuthenticationService authService,
	UsersMemoryRepository usersRepository,
	SessionManagerService sessionManager) : ControllerBase
{
	/// <summary>
	/// Аутентификация пользователя, прошедшего проверку на сервере EnergoId
	/// </summary>
	/// <param name="energoIdInfo">Данные пользователя Keycloak</param>
	/// <returns>Данные о учетной записи</returns>
	[HttpPost("energo-id")]
	public ActionResult<UserAuthInfo> AuthenticateEnergoIdUser(
		[BindRequired, FromBody] UserEnergoIdInfo energoIdInfo)
	{
		var userAuthInfo = authService.Authenticate(energoIdInfo);

		var session = sessionManager.OpenSession(userAuthInfo);
		sessionManager.AddSessionToResponse(session, Response);

		userAuthInfo.Token = session.Token;

		return userAuthInfo;
	}

	/// <summary>
	/// Аутентификация локального пользователя по связке "имя для входа/пароль"
	/// </summary>
	/// <param name="loginPass">Данные для входа</param>
	/// <returns>Данные о учетной записи</returns>
	[HttpPost("auth")]
	public ActionResult<UserAuthInfo> Authenticate(
		[BindRequired, FromBody] UserLoginPass loginPass)
	{
		var userAuthInfo = authService.Authenticate(loginPass);

		var session = sessionManager.OpenSession(userAuthInfo);
		sessionManager.AddSessionToResponse(session, Response);

		userAuthInfo.Token = session.Token;

		return userAuthInfo;
	}

	/// <summary>
	/// Получение информации о учетной записи на основе текущей сессии
	/// </summary>
	/// <returns>Данные о учетной записи</returns>
	[HttpGet("identify")]
	public ActionResult<UserAuthInfo> Identify()
	{
		var user = authenticator.Authenticate(HttpContext);

		return user;
	}

	/// <summary>
	/// Создание пользователя на основании переданных данных
	/// </summary>
	/// <param name="userAuthRequest">Данные нового пользователя</param>
	/// <returns>Идентификатор пользователя</returns>
	[HttpPost]
	public async Task<ActionResult<UserInfo>> CreateAsync(
		[BindRequired, FromBody] UserCreateRequest userAuthRequest)
	{
		var user = authenticator.Authenticate(HttpContext);

		var info = await usersRepository.CreateAsync(db, user, userAuthRequest);

		return info;
	}

	/// <summary>
	/// Получение списка пользователей
	/// </summary>
	/// <returns>Список пользователей</returns>
	[HttpGet]
	public ActionResult<UserInfo[]> Read()
	{
		var user = authenticator.Authenticate(HttpContext);

		return usersRepository.ReadAll(user);
	}

	/// <summary>
	/// Получение данных о пользователе
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <returns>Данные пользователя</returns>
	/// <exception cref="NotFoundException">Пользователь не найден по ключу</exception>
	[HttpGet("{userGuid}")]
	public ActionResult<UserInfo> Read(
		[BindRequired, FromRoute] Guid userGuid)
	{
		var user = authenticator.Authenticate(HttpContext);

		return usersRepository.Read(user, userGuid);
	}

	/// <summary>
	/// Получение детализированной информации о пользователе
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <returns>Данные о пользователе</returns>
	/// <exception cref="NotFoundException">Пользователь не найден по ключу</exception>
	[HttpGet("{userGuid}/detailed")]
	public ActionResult<UserDetailInfo> ReadWithDetails(
		[BindRequired, FromRoute] Guid userGuid)
	{
		var user = authenticator.Authenticate(HttpContext);

		return usersRepository.ReadWithDetails(user, userGuid);
	}

	/// <summary>
	/// Получение списка пользователей, определенных на сервере EnergoId
	/// </summary>
	/// <returns>Список пользователей</returns>
	[HttpGet("energo-id")]
	public ActionResult<UserEnergoIdInfo[]> ReadEnergoId()
	{
		var user = authenticator.Authenticate(HttpContext);

		return usersRepository.ReadEnergoId(user);
	}

	/// <summary>
	/// Изменение пользователя
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <param name="userUpdateRequest">Новые данные пользователя</param>
	[HttpPut("{userGuid}")]
	public async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] Guid userGuid,
		[BindRequired, FromBody] UserUpdateRequest userUpdateRequest)
	{
		var user = authenticator.Authenticate(HttpContext);

		await usersRepository.UpdateAsync(db, user, userGuid, userUpdateRequest);

		return NoContent();
	}

	/// <summary>
	/// Обновление данных из EnergoId
	/// </summary>
	[HttpPut("energo-id")]
	public ActionResult UpdateEnergoId()
	{
		var user = authenticator.Authenticate(HttpContext);

		usersRepository.UpdateEnergoId(user);

		return NoContent();
	}

	/// <summary>
	/// Удаление пользователя
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	[HttpDelete("{userGuid}")]
	public async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] Guid userGuid)
	{
		var user = authenticator.Authenticate(HttpContext);

		await usersRepository.DeleteAsync(db, user, userGuid);

		return NoContent();
	}
}
