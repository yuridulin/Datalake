using Datalake.Contracts.Models.Tags;
using Datalake.Data.Application.Features.Tags.Queries.GetCollectionStatus;
using Datalake.Data.Application.Features.Tags.Queries.GetUsage;
using Datalake.Shared.Hosting.AbstractControllers.Data;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Data.Host.Controllers;

public class TagsController(
	IServiceProvider serviceProvider,
	IAuthenticator authenticator) : DataTagsControllerBase
{
	public override async Task<ActionResult<IDictionary<int, string>>> GetStatusAsync(
		[BindRequired, FromBody] TagMetricRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetTagsCollectionStatusHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
			TagsId = request.TagsId ?? [],
		}, ct);

		return Ok(data);
	}

	public override async Task<ActionResult<IDictionary<int, IDictionary<string, DateTime>>>> GetUsageAsync(
		[BindRequired, FromBody] TagMetricRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetUsageHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
			TagsId = request.TagsId,
			TagsGuid = request.TagsGuid,
		}, ct);

		return Ok(data);
	}
}
