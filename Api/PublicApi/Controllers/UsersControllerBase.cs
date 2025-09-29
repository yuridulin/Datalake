using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Models.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.PublicApi.Controllers;

/// <summary>
/// Взаимодействие с пользователями
/// </summary>
[ApiController]
[Route($"{Defaults.ApiRoot}/{ControllerRoute}")]
public abstract class UsersControllerBase : ControllerBase
{
	#region Константы путей

	/// <summary>
	/// Основной путь к контроллеру
	/// </summary>
	public const string ControllerRoute = "users";

	/// <inheritdoc cref="CreateAsync(UserCreateRequest)" />
	public const string Create = "";

	/// <inheritdoc cref="GetAllAsync" />
	public const string GetAll = "";

	/// <inheritdoc cref="GetAsync(Guid)" />
	public const string Get = "{userGuid}";

	/// <inheritdoc cref="GetWithDetailsAsync(Guid)" />
	public const string GetWithDetails = "{userGuid}/detailed";

	/// <inheritdoc cref="GetEnergoIdAsync" />
	public const string GetEnergoId = "energo-id";

	/// <inheritdoc cref="UpdateAsync(Guid, UserUpdateRequest)" />
	public const string Update = "{userGuid}";

	/// <inheritdoc cref="UpdateEnergoIdAsync" />
	public const string UpdateEnergoId = "energo-id";

	/// <inheritdoc cref="DeleteAsync(Guid)" />
	public const string Delete = "{userGuid}";

	#endregion Константы путей

	#region Методы

	/// <summary>
	/// <see cref="HttpMethod.Post" />: Создание пользователя на основании переданных данных
	/// </summary>
	/// <param name="userAuthRequest">Данные нового пользователя</param>
	/// <returns>Идентификатор пользователя</returns>
	[HttpPost(Create)]
	public abstract Task<ActionResult<UserInfo>> CreateAsync(
		[BindRequired, FromBody] UserCreateRequest userAuthRequest);

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение списка пользователей
	/// </summary>
	/// <returns>Список пользователей</returns>
	[HttpGet(GetAll)]
	public abstract Task<ActionResult<UserInfo[]>> GetAllAsync();

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение данных о пользователе
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <returns>Данные пользователя</returns>
	/// <exception cref="NotFoundException">Пользователь не найден по ключу</exception>
	[HttpGet(Get)]
	public abstract Task<ActionResult<UserInfo>> GetAsync(
		[BindRequired, FromRoute] Guid userGuid);

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение детализированной информации о пользователе
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <returns>Данные о пользователе</returns>
	/// <exception cref="NotFoundException">Пользователь не найден по ключу</exception>
	[HttpGet(GetWithDetails)]
	public abstract Task<ActionResult<UserDetailInfo>> GetWithDetailsAsync(
		[BindRequired, FromRoute] Guid userGuid);

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение списка пользователей, определенных на сервере EnergoId
	/// </summary>
	/// <returns>Список пользователей</returns>
	[HttpGet(GetEnergoId)]
	public abstract Task<ActionResult<UserEnergoIdInfo[]>> GetEnergoIdAsync();

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Изменение пользователя
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <param name="userUpdateRequest">Новые данные пользователя</param>
	[HttpPut(Update)]
	public abstract Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] Guid userGuid,
		[BindRequired, FromBody] UserUpdateRequest userUpdateRequest);

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Обновление данных из EnergoId
	/// </summary>
	[HttpPut(UpdateEnergoId)]
	public abstract Task<ActionResult> UpdateEnergoIdAsync();

	/// <summary>
	/// <see cref="HttpMethod.Delete" />: Удаление пользователя
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	[HttpDelete(Delete)]
	public abstract Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] Guid userGuid);

	#endregion Методы
}