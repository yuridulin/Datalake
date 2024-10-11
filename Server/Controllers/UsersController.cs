using Datalake.ApiClasses.Exceptions;
using Datalake.ApiClasses.Models.Users;
using Datalake.Database;
using Datalake.Server.BackgroundServices.SettingsHandler;
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
	SessionManagerService sessionManager,
	ISettingsUpdater settingsService) : ApiControllerBase
{
	/// <summary>
	/// Получение списка пользователей, определенных на сервере EnergoId
	/// </summary>
	/// <returns>Список пользователей</returns>
	[HttpGet("energo-id")]
	public async Task<ActionResult<EnergoIdInfo>> GetEnergoIdListAsync(
		[FromQuery] Guid? currentUserGuid = null)
	{
		string energoId = await db.UsersRepository.GetEnergoIdApi();

		EnergoIdUserData[]? energoIdReceivedUsers = null;

		try
		{
			var clientHandler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
			};

			var client = new HttpClient(clientHandler);
			var users = await client.GetFromJsonAsync<EnergoIdUserData[]>("https://" + energoId);

			if (users != null)
				energoIdReceivedUsers = users;
		}
		catch { }

		if (energoIdReceivedUsers == null)
		{
			return new EnergoIdInfo();
		}

		var exists = await db.UsersRepository.GetFlatInfo()
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
		var userAuthInfo = await db.AccessRepository.AuthenticateAsync(energoIdInfo);

		var session = sessionManager.OpenSession(userAuthInfo);
		sessionManager.AddSessionToResponse(session, Response);

		userAuthInfo.Token = session.User.Token;

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
		var userAuthInfo = await db.AccessRepository.AuthenticateAsync(loginPass);

		var session = sessionManager.OpenSession(userAuthInfo);
		sessionManager.AddSessionToResponse(session, Response);

		userAuthInfo.Token = session.User.Token;

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
	public async Task<ActionResult<Guid>> CreateAsync(
		[BindRequired, FromBody] UserCreateRequest userAuthRequest)
	{
		var user = Authenticate();

		var guid = await db.UsersRepository.CreateAsync(user, userAuthRequest);
		settingsService.LoadStaticUsers(db.UsersRepository);

		return guid;
	}

	/// <summary>
	/// Получение списка пользователей
	/// </summary>
	/// <returns>Список пользователей</returns>
	[HttpGet]
	public async Task<ActionResult<UserInfo[]>> ReadAsync()
	{
		return await db.UsersRepository.GetInfo()
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
		return await db.UsersRepository.GetInfo()
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
		return await db.UsersRepository.GetDetailInfo()
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

		await db.UsersRepository.UpdateAsync(user, userGuid, userUpdateRequest);
		settingsService.LoadStaticUsers(db.UsersRepository);

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

		await db.UsersRepository.DeleteAsync(user, userGuid);
		settingsService.LoadStaticUsers(db.UsersRepository);

		return NoContent();
	}
}
