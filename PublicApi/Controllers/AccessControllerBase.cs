using Datalake.PublicApi.Models.AccessRights;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.PublicApi.Controllers;

/// <summary>
/// Работа с разрешениями
/// </summary>
[Route("api/" + ControllerRoute)]
[ApiController]
public abstract class AccessControllerBase : ControllerBase
{
	/// <summary>
	/// Основной путь к контроллеру
	/// </summary>
	public const string ControllerRoute = "access";

	/// <summary>
	/// Получение списка прямых (не глобальных) разрешений субъекта на объект
	/// </summary>
	/// <param name="user">Идентификтатор пользователя</param>
	/// <param name="userGroup">Идентификатор группы пользователей</param>
	/// <param name="source">Идентификатор источника</param>
	/// <param name="block">Идентификатор блока</param>
	/// <param name="tag">Идентификатор тега</param>
	/// <returns>Список разрешений</returns>
	[HttpGet]
	public abstract ActionResult<AccessRightsInfo[]> Get(
		[FromQuery] Guid? user = null,
		[FromQuery] Guid? userGroup = null,
		[FromQuery] int? source = null,
		[FromQuery] int? block = null,
		[FromQuery] int? tag = null);

	/// <summary>
	/// Изменение разрешений для группы пользователей
	/// </summary>
	/// <param name="request">Список изменений</param>
	[HttpPost]
	public abstract Task<ActionResult> ApplyChangesAsync(
		[FromBody] AccessRightsApplyRequest request);
}