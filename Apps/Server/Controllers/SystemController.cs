using Datalake.Database;
using Datalake.Database.Functions;
using Datalake.Database.InMemory;
using Datalake.Database.InMemory.Repositories;
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
	DatalakeDerivedDataStore derivedDataStore,
	DatalakeCurrentValuesStore valuesStore,
	SourcesStateService sourcesStateService,
	TagsStateService tagsStateService,
	UsersStateService usersStateService,
	SettingsMemoryRepository settingsRepository,
	RequestsStateService requestsStateService) : SystemControllerBase
{
	/// <inheritdoc />
	public override ActionResult<string> GetLastUpdate()
	{
		var lastUpdate = dataStore.State.Version;
		return lastUpdate.ToString();
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
	public override ActionResult<Dictionary<Guid, DateTime>> GetVisits()
	{
		var user = authenticator.Authenticate(HttpContext);

		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Viewer);

		return usersStateService.State();
	}

	/// <inheritdoc />
	public override ActionResult<Dictionary<int, SourceStateInfo>> GetSourcesStates()
	{
		var user = authenticator.Authenticate(HttpContext);

		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Viewer);

		return sourcesStateService.State()
			.Where(x => AccessChecks.HasAccessToSource(user, AccessType.Viewer, x.Key))
			.ToDictionary();
	}

	/// <inheritdoc />
	public override ActionResult<Dictionary<int, Dictionary<string, DateTime>>> GetTagsStates()
	{
		var user = authenticator.Authenticate(HttpContext);

		return tagsStateService.GetTagsStates()
			.Where(x => AccessChecks.HasAccessToTag(user, AccessType.Viewer, x.Key))
			.ToDictionary();
	}

	/// <inheritdoc />
	public override ActionResult<Dictionary<string, DateTime>> GetTagState(int id)
	{
		var user = authenticator.Authenticate(HttpContext);

		AccessChecks.ThrowIfNoAccessToTag(user, AccessType.Viewer, id);

		return tagsStateService.GetTagsStates().TryGetValue(id, out var state) ? state : [];
	}

	/// <inheritdoc />
	public override ActionResult<SettingsInfo> GetSettings()
	{
		var user = authenticator.Authenticate(HttpContext);

		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Editor);

		var info = settingsRepository.GetSettings(user);

		return info;
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
	public override ActionResult<Dictionary<Guid, UserAuthInfo>> GetAccess()
	{
		var user = authenticator.Authenticate(HttpContext);
		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		return derivedDataStore.Access.GetAll();
	}

	/// <inheritdoc />
	public override ActionResult<KeyValuePair<ValuesRequestKey, ValuesRequestUsageInfo>[]> GetReadMetricsAsync()
	{
		var user = authenticator.Authenticate(HttpContext);
		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		return requestsStateService.GetAllStats().ToArray();
	}
}