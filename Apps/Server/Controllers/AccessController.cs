using Datalake.Database;
using Datalake.Database.InMemory.Repositories;
using Datalake.PublicApi.Models.AccessRights;
using Datalake.Server.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Server.Controllers;

/// <summary>
/// Работа с разрешениями
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AccessController(
	DatalakeContext db,
	AuthenticationService authenticator,
	AccessRightsMemoryRepository accessRepository) : ControllerBase
{
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
	public ActionResult<AccessRightsInfo[]> Read(
		[FromQuery] Guid? user = null,
		[FromQuery] Guid? userGroup = null,
		[FromQuery] int? source = null,
		[FromQuery] int? block = null,
		[FromQuery] int? tag = null)
	{
		var userAuth = authenticator.Authenticate(HttpContext);

		return accessRepository.Read(
			user: userAuth,
			userGuid: user,
			userGroupGuid: userGroup,
			sourceId: source,
			blockId: block,
			tagId: tag);
	}

	/// <summary>
	/// Изменение разрешений для группы пользователей
	/// </summary>
	/// <param name="request">Список изменений</param>
	[HttpPost]
	public async Task<ActionResult> ApplyChangesAsync(
		[FromBody] AccessRightsApplyRequest request)
	{
		var user = authenticator.Authenticate(HttpContext);

		await accessRepository.ApplyChangesAsync(db, user, request);

		return NoContent();
	}
}
