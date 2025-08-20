using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.PublicApi.Controllers;

/// <summary>
/// Взаимодействие с пользователями
/// </summary>
[Route("api/" + ControllerRoute)]
[ApiController]
public abstract class UsersControllerBase : ControllerBase
{
	/// <summary>
	/// Основной путь к контроллеру
	/// </summary>
	public const string ControllerRoute = "users";

	/// <summary>
	/// Аутентификация пользователя, прошедшего проверку на сервере EnergoId
	/// </summary>
	/// <param name="energoIdInfo">Данные пользователя Keycloak</param>
	/// <returns>Данные о учетной записи</returns>
	[HttpPost("energo-id")]
	public abstract Task<ActionResult<UserAuthInfo>> AuthenticateEnergoIdUserAsync(
		[BindRequired, FromBody] UserEnergoIdInfo energoIdInfo);

	/// <summary>
	/// Аутентификация локального пользователя по связке "имя для входа/пароль"
	/// </summary>
	/// <param name="loginPass">Данные для входа</param>
	/// <returns>Данные о учетной записи</returns>
	[HttpPost("auth")]
	public abstract Task<ActionResult<UserAuthInfo>> AuthenticateAsync(
		[BindRequired, FromBody] UserLoginPass loginPass);

	/// <summary>
	/// Получение информации о учетной записи на основе текущей сессии
	/// </summary>
	/// <returns>Данные о учетной записи</returns>
	[HttpGet("identify")]
	public abstract Task<ActionResult<UserAuthInfo>> IdentifyAsync();

	/// <summary>
	/// Создание пользователя на основании переданных данных
	/// </summary>
	/// <param name="userAuthRequest">Данные нового пользователя</param>
	/// <returns>Идентификатор пользователя</returns>
	[HttpPost]
	public abstract Task<ActionResult<UserInfo>> CreateAsync(
		[BindRequired, FromBody] UserCreateRequest userAuthRequest);

	/// <summary>
	/// Получение списка пользователей
	/// </summary>
	/// <returns>Список пользователей</returns>
	[HttpGet]
	public abstract Task<ActionResult<UserInfo[]>> GetAllAsync();

	/// <summary>
	/// Получение данных о пользователе
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <returns>Данные пользователя</returns>
	/// <exception cref="NotFoundException">Пользователь не найден по ключу</exception>
	[HttpGet("{userGuid}")]
	public abstract Task<ActionResult<UserInfo>> GetAsync(
		[BindRequired, FromRoute] Guid userGuid);

	/// <summary>
	/// Получение детализированной информации о пользователе
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <returns>Данные о пользователе</returns>
	/// <exception cref="NotFoundException">Пользователь не найден по ключу</exception>
	[HttpGet("{userGuid}/detailed")]
	public abstract Task<ActionResult<UserDetailInfo>> GetWithDetailsAsync(
		[BindRequired, FromRoute] Guid userGuid);

	/// <summary>
	/// Получение списка пользователей, определенных на сервере EnergoId
	/// </summary>
	/// <returns>Список пользователей</returns>
	[HttpGet("energo-id")]
	public abstract Task<ActionResult<UserEnergoIdInfo[]>> GetEnergoIdAsync();

	/// <summary>
	/// Изменение пользователя
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <param name="userUpdateRequest">Новые данные пользователя</param>
	[HttpPut("{userGuid}")]
	public abstract Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] Guid userGuid,
		[BindRequired, FromBody] UserUpdateRequest userUpdateRequest);

	/// <summary>
	/// Обновление данных из EnergoId
	/// </summary>
	[HttpPut("energo-id")]
	public abstract Task<ActionResult> UpdateEnergoIdAsync();

	/// <summary>
	/// Удаление пользователя
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	[HttpDelete("{userGuid}")]
	public abstract Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] Guid userGuid);
}