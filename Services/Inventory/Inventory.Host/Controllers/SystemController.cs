using Datalake.Contracts.Models.Settings;
using Datalake.Inventory.Application.Features.Cache.Commands.ReloadCache;
using Datalake.Inventory.Application.Features.Settings.Commands.UpdateSettings;
using Datalake.Inventory.Application.Features.Settings.Queries.GetSettings;
using Datalake.Shared.Hosting.Controllers.Inventory;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Inventory.Host.Controllers;

public class SystemController(
	IServiceProvider serviceProvider,
	IAuthenticator authenticator) : InventorySystemControllerBase
{
	public override async Task<ActionResult<SettingsInfo>> GetSettingsAsync(
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetSettingsHandler>();
		var data = await handler.HandleAsync(new(user), ct);

		return data;
	}

	public override async Task<ActionResult> UpdateSettingsAsync(
		[BindRequired][FromBody] SettingsInfo newSettings,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IUpdateSettingsHandler>();
		await handler.HandleAsync(new(
			user,
			KeycloakClient: newSettings.EnergoIdClient,
			KeycloakHost: newSettings.EnergoIdHost,
			EnergoIdApi: newSettings.EnergoIdApi,
			InstanceName: newSettings.InstanceName), ct);

		return NoContent();
	}

	public override async Task<ActionResult> RestartStateAsync(
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IReloadCacheHandler>();
		await handler.HandleAsync(new() { User = user }, ct);

		return NoContent();
	}
}