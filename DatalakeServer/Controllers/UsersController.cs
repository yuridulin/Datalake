using DatalakeApiClasses.Exceptions;
using DatalakeApiClasses.Models.Users;
using DatalakeDatabase.Repositories;
using DatalakeServer.Controllers.Base;
using DatalakeServer.Services.SessionManager;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DatalakeServer.Controllers;

/// <summary>
/// Взаимодействие с пользователями
/// </summary>
/// <param name="usersRepository">Репозиторий</param>
/// <param name="sessionManager">Менеджер сессий</param>
[Route("api/[controller]")]
[ApiController]
public class UsersController(
	UsersRepository usersRepository,
	SessionManagerService sessionManager) : ApiControllerBase
{
	/// <summary>
	/// Аутентификация пользователя, прошедшего проверку на сервере Keycloak
	/// </summary>
	/// <param name="energoIdInfo">Данные пользователя Keycloak</param>
	/// <returns>Данные о пользователе</returns>
	[HttpPost("energo-id")]
	public async Task<ActionResult<UserAuthInfo>> AuthenticateEnergoIdUserAsync(
		[BindRequired, FromBody] UserKeycloakInfo energoIdInfo)
	{
		var userAuthInfo = await usersRepository.AuthenticateAsync(energoIdInfo);

		var session = sessionManager.OpenSession(userAuthInfo);
		sessionManager.AddSessionToResponse(session, Response);

		userAuthInfo.Token = session.User.Token;

		return userAuthInfo;
	}

	/// <summary>
	/// Аутентификация локального пользователя по связке "имя для входа/пароль"
	/// </summary>
	/// <param name="loginPass">Данные для входа</param>
	/// <returns>Данные о пользователе</returns>
	[HttpPost("auth")]
	public async Task<ActionResult<UserAuthInfo>> AuthenticateAsync(
		[BindRequired, FromBody] UserLoginPass loginPass)
	{
		var userAuthInfo = await usersRepository.AuthenticateAsync(loginPass);

		var session = sessionManager.OpenSession(userAuthInfo);
		sessionManager.AddSessionToResponse(session, Response);

		userAuthInfo.Token = session.User.Token;

		return userAuthInfo;
	}

	/// <summary>
	/// Создание пользователя на основании переданных данных
	/// </summary>
	/// <param name="userAuthRequest">Данные нового пользователя</param>
	/// <returns>Идентификатор пользователя</returns>
	[HttpPost]
	public async Task<ActionResult<Guid>> CreateAsync(
		[BindRequired, FromBody] UserCreateRequest userAuthRequest)
	{
		var user = Authenticate();

		return await usersRepository.CreateAsync(user, userAuthRequest);
	}

	/// <summary>
	/// Получение списка пользователей
	/// </summary>
	/// <returns>Список пользователей</returns>
	[HttpGet]
	public async Task<ActionResult<UserInfo[]>> ReadAsync()
	{
		return await usersRepository.GetInfo()
			.ToArrayAsync();
	}

	/// <summary>
	/// Получение данных о пользователе
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <returns>Данные пользователя</returns>
	/// <exception cref="NotFoundException">Пользователь не найден по ключу</exception>
	[HttpGet("{userGuid}")]
	public async Task<ActionResult<UserInfo>> ReadAsync(
		[BindRequired, FromRoute] Guid userGuid)
	{
		return await usersRepository.GetInfo()
			.Where(x => x.Guid == userGuid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"Учётная запись {userGuid}");
	}

	/// <summary>
	/// Получение детализированной информации о пользователе
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <returns>Данные о пользователе</returns>
	/// <exception cref="NotFoundException">Пользователь не найден по ключу</exception>
	[HttpGet("{userGuid}/detailed")]
	public async Task<ActionResult<UserDetailInfo>> ReadWithDetailsAsync(
		[BindRequired, FromRoute] Guid userGuid)
	{
		return await usersRepository.GetDetailInfo()
			.Where(x => x.Guid == userGuid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"Учётная запись {userGuid}");
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
		var user = Authenticate();

		await usersRepository.UpdateAsync(user, userGuid, userUpdateRequest);

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
		var user = Authenticate();

		await usersRepository.DeleteAsync(user, userGuid);

		return NoContent();
	}
}
