using Datalake.Database.Functions;
using Datalake.PublicApi.Controllers;
using Datalake.PublicApi.Enums;
using Datalake.Server.Services.Auth;
using Datalake.Server.Services.Maintenance;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.InventoryService.Controllers;

/// <inheritdoc />
public class StatesController(
	AuthenticationService authenticator,
	UsersStateService usersStateService) : StatesControllerBase
{
	/// <inheritdoc />
	public override async Task<ActionResult<Dictionary<Guid, DateTime>>> GetUsersAsync()
	{
		var user = authenticator.Authenticate(HttpContext);

		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Viewer);

		return await Task.FromResult(usersStateService.State());
	}
}