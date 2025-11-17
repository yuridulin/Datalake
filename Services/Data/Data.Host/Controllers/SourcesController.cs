using Datalake.Contracts.Models.Sources;
using Datalake.Data.Application.Features.Sources.Queries.GetActivity;
using Datalake.Data.Application.Features.Sources.Queries.GetRemoteItems;
using Datalake.Shared.Hosting.Controllers.Data;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Data.Host.Controllers;

public class SourcesController(
	IServiceProvider serviceProvider,
	IAuthenticator authenticator) : DataSourcesControllerBase
{
	public override async Task<ActionResult<IEnumerable<SourceActivityInfo>>> GetActivityAsync(
		[BindRequired, FromBody] int[] sourcesId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetSourcesActivityHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
			SourcesId = sourcesId
		}, ct);

		return Ok(data);
	}

	public override async Task<ActionResult<IEnumerable<SourceItemInfo>>> GetItemsAsync(
		[BindRequired, FromRoute] int sourceId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetSourceRemoteItemsHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
			SourceId = sourceId
		}, ct);

		return Ok(data);
	}
}
