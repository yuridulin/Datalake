using Datalake.Database;
using Datalake.Database.Functions;
using Datalake.Database.InMemory.Repositories;
using Datalake.Database.InMemory.Stores;
using Datalake.Database.Repositories;
using Datalake.PublicApi.Controllers;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.LogModels;
using Datalake.PublicApi.Models.Settings;
using Datalake.PublicApi.Models.Sources;
using Datalake.PublicApi.Models.Values;
using Datalake.Server.Services.Auth;
using Datalake.Server.Services.Maintenance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Server.Controllers;

/// <inheritdoc />
public class SystemController(
	DatalakeContext db,
	AuthenticationService authenticator,
	DatalakeDataStore dataStore,
	DatalakeAccessStore accessStore,
	DatalakeCurrentValuesStore valuesStore,
	SourcesStateService sourcesStateService,
	TagsStateService tagsStateService,
	UsersStateService usersStateService,
	SettingsMemoryRepository settingsRepository,
	RequestsStateService requestsStateService) : SystemControllerBase
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

		return await AuditRepository.GetAsync(db, userAuth, lastId, firstId, take, source, block, tag, user, group, categories, types, author);
	}

	/// <inheritdoc />
	public override async Task<ActionResult<Dictionary<Guid, DateTime>>> GetVisitsAsync()
	{
		var user = authenticator.Authenticate(HttpContext);

		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Viewer);

		return await Task.FromResult(usersStateService.State());
	}

	/// <inheritdoc />
	public override async Task<ActionResult<Dictionary<int, SourceStateInfo>>> GetSourcesStatesAsync()
	{
		var user = authenticator.Authenticate(HttpContext);

		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Viewer);

		return await Task.FromResult(sourcesStateService.State()
			.Where(x => AccessChecks.HasAccessToSource(user, AccessType.Viewer, x.Key))
			.ToDictionary());
	}

	/// <inheritdoc />
	public override async Task<ActionResult<Dictionary<int, Dictionary<string, DateTime>>>> GetTagsStatesAsync()
	{
		var user = authenticator.Authenticate(HttpContext);

		return await Task.FromResult(tagsStateService.GetTagsStates()
			.Where(x => AccessChecks.HasAccessToTag(user, AccessType.Viewer, x.Key))
			.ToDictionary());
	}

	/// <inheritdoc />
	public override async Task<ActionResult<Dictionary<string, DateTime>>> GetTagStateAsync(int id)
	{
		var user = authenticator.Authenticate(HttpContext);

		AccessChecks.ThrowIfNoAccessToTag(user, AccessType.Viewer, id);

		return await Task.FromResult(tagsStateService.GetTagsStates().TryGetValue(id, out var state) ? state : []);
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
	public override async Task<ActionResult> RestartValuesAsync()
	{
		var user = authenticator.Authenticate(HttpContext);

		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		await valuesStore.ReloadValuesAsync();

		return NoContent();
	}

	/// <inheritdoc />
	public override async Task<ActionResult<Dictionary<Guid, UserAuthInfo>>> GetAccessAsync()
	{
		var user = authenticator.Authenticate(HttpContext);
		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		return await Task.FromResult(accessStore.State.GetAll());
	}

	/// <inheritdoc />
	public override async Task<ActionResult<KeyValuePair<ValuesRequestKey, ValuesRequestUsageInfo>[]>> GetReadMetricsAsync()
	{
		var user = authenticator.Authenticate(HttpContext);
		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		return await Task.FromResult(requestsStateService.GetAllStats().ToArray());
	}
}