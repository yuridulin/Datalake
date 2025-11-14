using Datalake.Contracts.Models.Users;
using Datalake.Inventory.Application.Features.EnergoId.Commands.ReloadEnergoId;
using Datalake.Inventory.Application.Features.EnergoId.Queries.GetEnergoId;
using Datalake.Shared.Hosting.Controllers.Inventory;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Inventory.Host.Controllers;

public class EnergoIdController(
	IServiceProvider serviceProvider,
	IAuthenticator authenticator) : InventoryEnergoIdControllerBase
{
	public override async Task<ActionResult<UserEnergoIdInfo[]>> GetEnergoIdAsync(
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetEnergoIdHandler>();
		var data = await handler.HandleAsync(new() { User = user }, ct);

		return Ok(data);
	}

	public override async Task<ActionResult> UpdateEnergoIdAsync(
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IReloadEnergoIdHandler>();
		await handler.HandleAsync(new() { User = user }, ct);

		return NoContent();
	}
}