using Datalake.Contracts.Models.Users;
using Datalake.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Shared.Hosting.AbstractControllers.Inventory;

/// <summary>
/// Учетные записи
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "Inventory")]
[Route("api/v1/inventory/users")]
public abstract class InventoryUsersControllerBase : ControllerBase
{
	/// <summary>
	/// Создание пользователя на основании переданных данных
	/// </summary>
	/// <param name="request">Данные нового пользователя</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Идентификатор пользователя</returns>
	[HttpPost]
	public abstract Task<ActionResult<Guid>> CreateAsync(
		[BindRequired, FromBody] UserCreateRequest request,
		CancellationToken ct = default);

	/// <summary>
	/// Получение списка пользователей
	/// </summary>
	/// <param name="userGuid">Идентификатор запрошенного пользователя</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список пользователей</returns>
	[HttpGet]
	public abstract Task<ActionResult<List<UserInfo>>> GetAsync(
		[FromQuery] Guid? userGuid,
		CancellationToken ct = default);

	/// <summary>
	/// Получение детализированной информации о пользователе
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Данные о пользователе</returns>
	[HttpGet("{userGuid}")]
	public abstract Task<ActionResult<UserWithGroupsInfo>> GetWithDetailsAsync(
		[BindRequired, FromRoute] Guid userGuid,
		CancellationToken ct = default);

	/// <summary>
	/// Изменение пользователя
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <param name="request">Новые данные пользователя</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("{userGuid}")]
	public abstract Task<ActionResult<bool>> UpdateAsync(
		[BindRequired, FromRoute] Guid userGuid,
		[BindRequired, FromBody] UserUpdateRequest request,
		CancellationToken ct = default);

	/// <summary>
	/// Удаление пользователя
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <param name="ct">Токен отмены</param>
	[HttpDelete("{userGuid}")]
	public abstract Task<ActionResult<bool>> DeleteAsync(
		[BindRequired, FromRoute] Guid userGuid,
		CancellationToken ct = default);
}
