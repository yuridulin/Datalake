using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Models.AccessRights;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.PublicApi.Controllers;

/// <summary>
/// Работа с разрешениями
/// </summary>
[ApiController]
[Route($"{Defaults.ApiRoot}/{ControllerRoute}")]
public abstract class AccessControllerBase : ControllerBase
{
	#region Константы путей

	/// <summary>
	/// Основной путь к контроллеру
	/// </summary>
	public const string ControllerRoute = "access";

	/// <inheritdoc cref="GetAsync" />
	public const string Get = "";

	/// <inheritdoc cref="ApplyChangesAsync" />
	public const string Apply = "";

	#endregion Константы путей

	#region Методы

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение списка прямых (не глобальных) разрешений субъекта на объект
	/// </summary>
	/// <param name="user">Идентификтатор пользователя</param>
	/// <param name="userGroup">Идентификатор группы пользователей</param>
	/// <param name="source">Идентификатор источника</param>
	/// <param name="block">Идентификатор блока</param>
	/// <param name="tag">Идентификатор тега</param>
	/// <returns>Список разрешений</returns>
	[HttpGet(Get)]
	public abstract Task<ActionResult<AccessRightsInfo[]>> GetAsync(
		[FromQuery] Guid? user = null,
		[FromQuery] Guid? userGroup = null,
		[FromQuery] int? source = null,
		[FromQuery] int? block = null,
		[FromQuery] int? tag = null);

	/// <summary>
	/// <see cref="HttpMethod.Post" />: Изменение разрешений для группы пользователей
	/// </summary>
	/// <param name="request">Список изменений</param>
	[HttpPost(Apply)]
	public abstract Task<ActionResult> ApplyChangesAsync(
		[FromBody] AccessRightsApplyRequest request);

	#endregion Методы
}
