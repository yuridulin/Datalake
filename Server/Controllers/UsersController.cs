using Datalake.Database;
using Datalake.Database.Repositories;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Users;
using Datalake.Server.Controllers.Base;
using Datalake.Server.Models;
using Datalake.Server.Services.SessionManager;
using LinqToDB;
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
	SessionManagerService sessionManager) : ApiControllerBase
{
	/// <summary>
	/// Получение списка пользователей, определенных на сервере EnergoId
	/// </summary>
	/// <returns>Список пользователей</returns>
	[HttpGet("energo-id")]
	public async Task<ActionResult<EnergoIdInfo>> GetEnergoIdListAsync(
		[FromQuery] Guid? currentUserGuid = null)
	{
		var user = Authenticate();

		AccessRepository.HasGlobalAccess(user, PublicApi.Enums.AccessType.Admin);

		var settings = await SystemRepository.GetSettingsAsSystemAsync(db);

		EnergoIdUserData[]? energoIdReceivedUsers = null;

		try
		{
			var clientHandler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
			};

			var client = new HttpClient(clientHandler);
			var users = await client.GetFromJsonAsync<EnergoIdUserData[]>("https://" + settings.EnergoIdHost);

			if (users != null)
				energoIdReceivedUsers = users;
		}
		catch { }

		if (energoIdReceivedUsers == null)
		{
			return new EnergoIdInfo();
		}

		var exists = await UsersRepository.GetFlatInfo(db)
			.Where(x => x.EnergoIdGuid != null && (currentUserGuid == null || x.Guid != currentUserGuid))
			.Select(x => x.EnergoIdGuid.ToString())
			.ToArrayAsync();

		var response = new EnergoIdInfo
		{
			Connected = true,
			EnergoIdUsers = energoIdReceivedUsers
				.ExceptBy(exists, u => u.Sid)
				.Select(x => new UserEnergoIdInfo
				{
					EnergoIdGuid = Guid.TryParse(x.Sid, out var guid) ? guid : Guid.Empty,
					Login = x.Email,
					FullName = x.Name,
				})
				.Where(x => x.EnergoIdGuid != Guid.Empty)
				.ToArray()
		};

		return response;
	}

	/// <summary>
	/// Аутентификация пользователя, прошедшего проверку на сервере EnergoId
	/// </summary>
	/// <param name="energoIdInfo">Данные пользователя Keycloak</param>
	/// <returns>Данные о учетной записи</returns>
	[HttpPost("energo-id")]
	public async Task<ActionResult<UserAuthInfo>> AuthenticateEnergoIdUserAsync(
		[BindRequired, FromBody] UserEnergoIdInfo energoIdInfo)
	{
		var userAuthInfo = await AccessRepository.AuthenticateAsync(db, energoIdInfo);

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
	public async Task<ActionResult<UserAuthInfo>> AuthenticateAsync(
		[BindRequired, FromBody] UserLoginPass loginPass)
	{
		var userAuthInfo = await AccessRepository.AuthenticateAsync(db, loginPass);

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
		var user = Authenticate();

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
		var user = Authenticate();

		var info = await UsersRepository.CreateAsync(db, user, userAuthRequest);

		return info;
	}

	/// <summary>
	/// Получение списка пользователей
	/// </summary>
	/// <returns>Список пользователей</returns>
	[HttpGet]
	public async Task<ActionResult<UserInfo[]>> ReadAsync()
	{
		var user = Authenticate();

		return await UsersRepository.ReadAllAsync(db, user);
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
		var user = Authenticate();

		return await UsersRepository.ReadAsync(db, user, userGuid);
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
		var user = Authenticate();

		return await UsersRepository.ReadWithDetailsAsync(db, user, userGuid);
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

		await UsersRepository.UpdateAsync(db, user, userGuid, userUpdateRequest);

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

		await UsersRepository.DeleteAsync(db, user, userGuid);

		return NoContent();
	}
}
