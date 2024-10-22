using Datalake.ApiClasses.Models.AccessRights;
using Datalake.Database;
using Datalake.Server.Controllers.Base;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Server.Controllers;

/// <summary>
/// Работа с разрешениями
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AccessController(DatalakeContext db) : ApiControllerBase
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
	public async Task<ActionResult<AccessRightsInfo[]>> GetAsync(
		[FromQuery] Guid? user = null,
		[FromQuery] Guid? userGroup = null,
		[FromQuery] int? source = null,
		[FromQuery] int? block = null,
		[FromQuery] int? tag = null)
	{
		var query = db.AccessRepository.GetAccessRightsInfo(
			userGuid: user,
			userGroupGuid: userGroup,
			sourceId: source,
			blockId: block,
			tagId: tag);

		return await query.ToArrayAsync();
	}

	/// <summary>
	/// Получение разрешений пользователя на конкретные объекты, включая непрямые (предоставленные на объекты выше в иерархии либо на группы пользователя)
	/// </summary>
	/// <param name="user">Идентификатор пользователя</param>
	/// <param name="sources">Идентификаторы источников, разрешения на которые нужно проверить</param>
	/// <param name="blocks">Идентификаторы блоков, разрешения на которые нужно проверить</param>
	/// <param name="tags">Идентификаторы тегов, разрешения на которые нужно проверить</param>
	/// <returns>Список разрешений на доступ к запрошенным объектам</returns>
	[HttpGet("{user}")]
	public Task<ActionResult<AccessRightsForOneInfo[]>> CheckUserAccessAsync(
		[FromRoute] Guid user,
		[FromQuery] int[]? sources = null,
		[FromQuery] int[]? blocks = null,
		[FromQuery] int[]? tags = null)
	{
		throw new NotImplementedException();
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

		await db.AccessRepository.ApplyChangesAsync(user, request);

		return NoContent();
	}
}
