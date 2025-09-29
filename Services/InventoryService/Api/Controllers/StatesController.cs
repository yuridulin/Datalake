using Datalake.PrivateApi.Interfaces;
using Datalake.PublicApi.Controllers;
using Datalake.PublicApi.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.InventoryService.Api.Controllers;

/// <inheritdoc />
public class StatesController(IAuthenticator authenticator) : StatesControllerBase
{
	/// <inheritdoc />
	public override async Task<ActionResult<Dictionary<Guid, DateTime>>> GetUsersAsync()
	{
		var user = authenticator.Authenticate(HttpContext);

		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Viewer);

		return await Task.FromResult(usersStateService.State());
	}
}