using Datalake.Contracts.Models.Sources;
using Datalake.Contracts.Requests;
using Datalake.Inventory.Application.Features.Sources.Commands.CreateSource;
using Datalake.Inventory.Application.Features.Sources.Commands.DeleteSource;
using Datalake.Inventory.Application.Features.Sources.Commands.UpdateSource;
using Datalake.Inventory.Application.Features.Sources.Queries.GetSource;
using Datalake.Inventory.Application.Features.Sources.Queries.GetSources;
using Datalake.Shared.Hosting.AbstractControllers.Inventory;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Inventory.Host.Controllers;

public class SourcesController(
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

		return Ok(result);
	}

	public override async Task<ActionResult<SourceInfo>> GetAsync(
		[BindRequired, FromRoute] int sourceId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetSourceHandler>();
		var data = await handler.HandleAsync(new() { User = user, SourceId = sourceId }, ct);

		return Ok(data);
	}

	public override async Task<ActionResult<IEnumerable<SourceInfo>>> GetAllAsync(
		[FromQuery] bool withCustom = false,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetSourcesHandler>();
		var data = await handler.HandleAsync(new() { User = user, WithCustom = withCustom }, ct);

		return Ok(data);
	}

	public override async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int sourceId,
		[BindRequired, FromBody] SourceUpdateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IUpdateSourceHandler>();
		await handler.HandleAsync(new()
		{
			User = user,
			SourceId = sourceId,
			Name = request.Name,
			Description = request.Description,
			Address = request.Address,
			Type = request.Type,
			IsDisabled = request.IsDisabled,
		}, ct);

		return Ok();
	}

	public override async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int sourceId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IDeleteSourceHandler>();
		await handler.HandleAsync(new()
		{
			User = user,
			SourceId = sourceId
		}, ct);

		return Ok();
	}
}