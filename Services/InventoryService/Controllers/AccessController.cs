using Datalake.InventoryService.Services;
using Datalake.InventoryService.Services.Auth;
using Datalake.PublicApi.Models.AccessRights;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.InventoryService.Controllers;

/// <inheritdoc />
public class AccessController(
	AuthenticationService authenticator,
	AccessRightsService accessRightsService) : ControllerBase
{
	/*public async Task<ActionResult<AccessRightsInfo[]>> GetAsync(
		[FromQuery] Guid? user = null,
		[FromQuery] Guid? userGroup = null,
		[FromQuery] int? source = null,
		[FromQuery] int? block = null,
		[FromQuery] int? tag = null)
	{
		var userAuth = authenticator.Authenticate(HttpContext);

		return await Task.FromResult(accessRepository.Get(
			user: userAuth,
			userGuid: user,
			userGroupGuid: userGroup,
			sourceId: source,
			blockId: block,
			tagId: tag));
	}*/

	public async Task<ActionResult<bool>> ApplyChangesAsync(
		[FromBody] AccessRightsApplyRequest request)
	{
		var user = authenticator.Authenticate(HttpContext);

		return Ok(await accessRightsService.ApplyChangesAsync(user, request));
	}
}