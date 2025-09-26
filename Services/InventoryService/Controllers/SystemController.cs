using Datalake.Inventory.Functions;
using Datalake.Inventory;
using Datalake.PublicApi.Controllers;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.LogModels;
using Datalake.PublicApi.Models.Settings;
using Datalake.InventoryService.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Datalake.InventoryService.Database.Repositories;
using Datalake.InventoryService.InMemory.Stores;
using Datalake.InventoryService.InMemory.Repositories;

namespace Datalake.InventoryService.Controllers;

/// <inheritdoc />
public class SystemController(
	InventoryEfContext db,
	AuthenticationService authenticator,
	DatalakeDataStore dataStore,
	DatalakeAccessStore accessStore,
	SettingsMemoryRepository settingsRepository) : SystemControllerBase
{
	/// <inheritdoc />
	public override async Task<ActionResult<string>> GetLastUpdateAsync()
	{
		var lastUpdate = dataStore.State.Version;
		return await Task.FromResult(lastUpdate.ToString());
	}

	/// <inheritdoc />
	public override async Task<ActionResult<LogInfo[]>> GetLogsAsync(
		[FromQuery] int? lastId = null,
		[FromQuery] int? firstId = null,
		[FromQuery] int? take = null,
		[FromQuery] int? source = null,
		[FromQuery] int? block = null,
		[FromQuery] Guid? tag = null,
		[FromQuery] Guid? user = null,
		[FromQuery] Guid? group = null,
		[FromQuery(Name = nameof(categories) + "[]")] LogCategory[]? categories = null,
		[FromQuery(Name = nameof(types) + "[]")] LogType[]? types = null,
		[FromQuery] Guid? author = null)
	{
		var userAuth = authenticator.Authenticate(HttpContext);

		return await LogsRepository.GetAsync(db, userAuth, lastId, firstId, take, source, block, tag, user, group, categories, types, author);
	}


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

		await dataStore.ReloadStateAsync();

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