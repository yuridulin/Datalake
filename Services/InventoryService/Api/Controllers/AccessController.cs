using Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeBlockRules;
using Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeSourceRules;
using Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeTagRules;
using Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeUserGroupRules;
using Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeUserRules;
using Datalake.InventoryService.Application.Features.AccessRules.DTOs;
using Datalake.InventoryService.Application.Features.AccessRules.Queries.GetAccessRules;
using Datalake.PrivateApi.Interfaces;
using Datalake.PublicApi.Models.AccessRights;
using Datalake.PublicApi.Models.AccessRules;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.InventoryService.Api.Controllers;

/// <summary>
/// Работа с разрешениями
/// </summary>
[ApiController]
[Route("api/v1/access")]
public class AccessController(IAuthenticator authenticator) : ControllerBase
{
	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение списка прямых (не глобальных) разрешений субъекта на объект
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="userGuid">Идентификтатор пользователя</param>
	/// <param name="userGroupGuid">Идентификатор группы пользователей</param>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="tagId">Идентификатор тега</param>
	/// <returns>Список разрешений</returns>
	[HttpGet]
	public async Task<ActionResult<AccessRightsInfo[]>> GetAsync(
		[FromServices] IGetAccessRulesQueryHandler handler,
		[FromQuery(Name = "user")] Guid? userGuid = null,
		[FromQuery(Name = "userGroup")] Guid? userGroupGuid = null,
		[FromQuery(Name = "source")] int? sourceId = null,
		[FromQuery(Name = "block")] int? blockId = null,
		[FromQuery(Name = "tag")] int? tagId = null)
	{
		_ = authenticator.Authenticate(HttpContext);

		var data = await handler.HandleAsync(new(userGuid, userGroupGuid, sourceId, blockId, tagId));

		return Ok(data);
	}

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Изменение разрешений для учетной записи
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="userGuid">Идентификатор учетной записи</param>
	/// <param name="requests">Список изменений</param>
	[HttpPut("user/{userGuid}")]
	public async Task<ActionResult> SetUserRulesAsync(
		[FromServices] IChangeUserRulesCommandHandler handler,
		[FromRoute] Guid userGuid,
		[FromBody] AccessRuleForActorRequest[] requests)
	{
		var user = authenticator.Authenticate(HttpContext);

		await handler.HandleAsync(new(
			user,
			userGuid,
			requests.Select(x => new ActorRuleDto(
				x.AccessType,
				SourceId: x.SourceId,
				TagId: x.TagId,
				BlockId: x.BlockId))));

		return NoContent();
	}

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Изменение разрешений для группы учетных записей
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="userGroupGuid">Идентификатор группы учетных записей</param>
	/// <param name="requests">Список изменений</param>
	[HttpPut("user-group/{userGroupGuid}")]
	public async Task<ActionResult> SetUserGroupRulesAsync(
		[FromServices] IChangeUserGroupRulesCommandHandler handler,
		[FromRoute] Guid userGroupGuid,
		[FromBody] AccessRuleForActorRequest[] requests)
	{
		var user = authenticator.Authenticate(HttpContext);

		await handler.HandleAsync(new(
			user,
			userGroupGuid,
			requests.Select(x => new ActorRuleDto(
				x.AccessType,
				SourceId: x.SourceId,
				TagId: x.TagId,
				BlockId: x.BlockId))));

		return NoContent();
	}

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Изменение разрешений на источник данных
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="sourceId">Идентификатор источника данных</param>
	/// <param name="requests">Список изменений</param>
	[HttpPut("source/{sourceId}")]
	public async Task<ActionResult> SetSourceRulesAsync(
		[FromServices] IChangeSourceRulesCommandHandler handler,
		[FromRoute] int sourceId,
		[FromBody] AccessRuleForObjectRequest[] requests)
	{
		var user = authenticator.Authenticate(HttpContext);

		await handler.HandleAsync(new(
			user,
			sourceId,
			requests.Select(x => new ObjectRuleDto(
				x.AccessType,
				UserGuid: x.UserGuid,
				UserGroupGuid: x.UserGroupGuid))));

		return NoContent();
	}

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Изменение разрешений для блок
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="requests">Список изменений</param>
	[HttpPut("block/{blockId}")]
	public async Task<ActionResult> SetBlockRulesAsync(
		[FromServices] IChangeBlockRulesCommandHandler handler,
		[FromRoute] int blockId,
		[FromBody] AccessRuleForObjectRequest[] requests)
	{
		var user = authenticator.Authenticate(HttpContext);

		await handler.HandleAsync(new(
			user,
			blockId,
			requests.Select(x => new ObjectRuleDto(
				x.AccessType,
				UserGuid: x.UserGuid,
				UserGroupGuid: x.UserGroupGuid))));

		return NoContent();
	}

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Изменение разрешений для тега
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="requests">Список изменений</param>
	[HttpPut("tag/{tagId}")]
	public async Task<ActionResult> SetTagRulesAsync(
		[FromServices] IChangeTagRulesCommandHandler handler,
		[FromRoute] int tagId,
		[FromBody] AccessRuleForObjectRequest[] requests)
	{
		var user = authenticator.Authenticate(HttpContext);

		await handler.HandleAsync(new(
			user,
			tagId,
			requests.Select(x => new ObjectRuleDto(
				x.AccessType,
				UserGuid: x.UserGuid,
				UserGroupGuid: x.UserGroupGuid))));

		return NoContent();
	}
}
