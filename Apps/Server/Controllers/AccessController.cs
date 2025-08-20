using Datalake.Database;
using Datalake.Database.InMemory.Repositories;
using Datalake.PublicApi.Controllers;
using Datalake.PublicApi.Models.AccessRights;
using Datalake.Server.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Server.Controllers;

/// <inheritdoc />
public class AccessController(
	DatalakeContext db,
	AuthenticationService authenticator,
	AccessRightsMemoryRepository accessRepository) : AccessControllerBase
{
	/// <inheritdoc />
	public override ActionResult<AccessRightsInfo[]> Get(
		[FromQuery] Guid? user = null,
		[FromQuery] Guid? userGroup = null,
		[FromQuery] int? source = null,
		[FromQuery] int? block = null,
		[FromQuery] int? tag = null)
	{
		var userAuth = authenticator.Authenticate(HttpContext);

		return accessRepository.Get(
			user: userAuth,
			userGuid: user,
			userGroupGuid: userGroup,
			sourceId: source,
			blockId: block,
			tagId: tag);
	}

	/// <inheritdoc />
	public override async Task<ActionResult> ApplyChangesAsync(
		[FromBody] AccessRightsApplyRequest request)
	{
		var user = authenticator.Authenticate(HttpContext);

		await accessRepository.ApplyChangesAsync(db, user, request);

		return NoContent();
	}
}