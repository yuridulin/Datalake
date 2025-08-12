using Datalake.Database;
using Datalake.Database.InMemory;
using Datalake.Database.InMemory.Repositories;
using Datalake.PublicApi.Models.AccessRights;
using Datalake.Server.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Server.Controllers;

/// <summary>
/// Работа с разрешениями
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AccessController(
	DatalakeContext db,
	DatalakeDerivedDataStore derivedDataStore,
	AccessRightsMemoryRepository accessRepository) : ApiControllerBase(derivedDataStore)
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
		var userAuth = Authenticate();

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
		var user = Authenticate();

		await accessRepository.ApplyChangesAsync(db, user, request);

		return NoContent();
	}
}
