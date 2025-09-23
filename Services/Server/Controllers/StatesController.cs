using Datalake.Database.Functions;
using Datalake.PublicApi.Controllers;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Sources;
using Datalake.PublicApi.Models.States;
using Datalake.PublicApi.Models.Values;
using Datalake.Server.Services.Auth;
using Datalake.Server.Services.Maintenance;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Server.Controllers;

/// <inheritdoc />
public class StatesController(
	AuthenticationService authenticator,
	SourcesStateService sourcesStateService,
	TagsStateService tagsStateService,
	UsersStateService usersStateService,
	RequestsStateService requestsStateService,
	TagsReceiveStateService receiveStateService) : StatesControllerBase
{
	/// <inheritdoc />
	public override async Task<ActionResult<Dictionary<Guid, DateTime>>> GetUsersAsync()
	{
		var user = authenticator.Authenticate(HttpContext);

		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Viewer);

		return await Task.FromResult(usersStateService.State());
	}

	/// <inheritdoc />
	public override async Task<ActionResult<Dictionary<int, SourceStateInfo>>> GetSourcesAsync()
	{
		var user = authenticator.Authenticate(HttpContext);

		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Viewer);

		return await Task.FromResult(sourcesStateService.State()
			.Where(x => AccessChecks.HasAccessToSource(user, AccessType.Viewer, x.Key))
			.ToDictionary());
	}

	/// <inheritdoc />
	public override async Task<ActionResult<Dictionary<int, Dictionary<string, DateTime>>>> GetTagsAsync()
	{
		var user = authenticator.Authenticate(HttpContext);

		return await Task.FromResult(tagsStateService.GetTagsStates()
			.Where(x => AccessChecks.HasAccessToTag(user, AccessType.Viewer, x.Key))
			.ToDictionary());
	}

	/// <inheritdoc />
	public override async Task<ActionResult<Dictionary<string, DateTime>>> GetTagAsync(int id)
	{
		var user = authenticator.Authenticate(HttpContext);

		AccessChecks.ThrowIfNoAccessToTag(user, AccessType.Viewer, id);

		return await Task.FromResult(tagsStateService.GetTagsStates().TryGetValue(id, out var state) ? state : []);
	}

	/// <inheritdoc />
	public override async Task<ActionResult<KeyValuePair<ValuesRequestKey, ValuesRequestUsageInfo>[]>> GetValuesAsync()
	{
		var user = authenticator.Authenticate(HttpContext);
		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Viewer);

		return await Task.FromResult(requestsStateService.GetAllStats().ToArray());
	}

	/// <inheritdoc />
	public override async Task<ActionResult<Dictionary<int, TagReceiveState?>>> GetTagsReceiveAsync(
		[FromBody] int[]? identifiers)
	{
		var user = authenticator.Authenticate(HttpContext);
		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Viewer);

		return await Task.FromResult(receiveStateService.Get(identifiers));
	}
}