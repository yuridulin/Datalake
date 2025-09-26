using Datalake.Inventory.Functions;
using Datalake.PublicApi.Controllers;
using Datalake.PublicApi.Enums;
using Datalake.InventoryService.Services.Auth;
using Datalake.InventoryService.Services.Maintenance;
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