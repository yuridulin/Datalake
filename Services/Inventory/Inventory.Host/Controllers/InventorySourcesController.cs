using Datalake.Contracts.Models.Sources;
using Datalake.Contracts.Requests;
using Datalake.Inventory.Application.Features.Sources.Commands.CreateSource;
using Datalake.Inventory.Application.Features.Sources.Commands.DeleteSource;
using Datalake.Inventory.Application.Features.Sources.Commands.UpdateSource;
using Datalake.Inventory.Application.Features.Sources.Queries.GetSourcesWithSettings;
using Datalake.Inventory.Application.Features.Sources.Queries.GetSourceWithSettingsAndTags;
using Datalake.Shared.Hosting.AbstractControllers.Inventory;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Inventory.Host.Controllers;

public class InventorySourcesController(
	IServiceProvider serviceProvider,
	IAuthenticator authenticator) : InventorySourcesControllerBase
{
	public override async Task<ActionResult<int>> CreateAsync(
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<ICreateSourceHandler>();
		var result = await handler.HandleAsync(new()
		{
			User = user,
		}, ct);

		return result;
	}

	public override async Task<ActionResult<SourceWithSettingsAndTagsInfo>> GetAsync(
		[BindRequired, FromRoute] int sourceId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetSourceWithSettingsAndTagsHandler>();
		var data = await handler.HandleAsync(new() { User = user, SourceId = sourceId }, ct);

		return data;
	}

	public override async Task<ActionResult<IEnumerable<SourceWithSettingsInfo>>> GetAllAsync(
		[FromQuery] bool withCustom = false,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetSourcesWithSettingsHandler>();
		var data = await handler.HandleAsync(new() { User = user, WithCustom = withCustom }, ct);

		return data;
	}

	public override async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int sourceId,
		[BindRequired, FromBody] SourceUpdateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IUpdateSourceHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
			SourceId = sourceId,
			Name = request.Name,
			Description = request.Description,
			Address = request.Address,
			Type = request.Type,
			IsDisabled = request.IsDisabled,
		}, ct);

		return data;
	}

	public override async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int sourceId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IDeleteSourceHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
			SourceId = sourceId
		}, ct);

		return data;
	}
}