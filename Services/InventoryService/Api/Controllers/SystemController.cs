using Datalake.InventoryService.Api.Services;
using Datalake.InventoryService.Application.Features.Settings;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Cache.UserAccess;
using Datalake.PublicApi.Controllers;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.InventoryService.Api.Controllers;

/// <inheritdoc />
public class SystemController(
	AuthenticationService authenticator,
	InventoryCacheStore dataStore,
	UserAccessCacheStore accessStore,
	SettingsMemoryRepository settingsRepository) : SystemControllerBase
{
	/// <inheritdoc />
	public override async Task<ActionResult<string>> GetLastUpdateAsync()
	{
		var lastUpdate = dataStore.State.Version;
		return await Task.FromResult(lastUpdate.ToString());
	}

	/// <inheritdoc />


	/// <inheritdoc />
	public override async Task<ActionResult<SettingsInfo>> GetSettingsAsync()
	{
		var user = authenticator.Authenticate(HttpContext);

		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Editor);

		var info = settingsRepository.GetSettings(user);

		return await Task.FromResult(info);
	}

	/// <inheritdoc />
	public override async Task<ActionResult> UpdateSettingsAsync(
		[BindRequired][FromBody] SettingsInfo newSettings)
	{
		var user = authenticator.Authenticate(HttpContext);

		await settingsRepository.UpdateSettingsAsync(db, user, newSettings);

		return NoContent();
	}

	/// <inheritdoc />
	public override async Task<ActionResult> RestartStateAsync()
	{
		var user = authenticator.Authenticate(HttpContext);

		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		await dataStore.RestoreAsync();

		return NoContent();
	}

	/// <inheritdoc />
	public override async Task<ActionResult<Dictionary<Guid, UserAuthInfo>>> GetAccessAsync()
	{
		var user = authenticator.Authenticate(HttpContext);
		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		return await Task.FromResult(accessStore.State.GetAll());
	}
}