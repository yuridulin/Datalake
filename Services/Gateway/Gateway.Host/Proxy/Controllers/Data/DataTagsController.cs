using Datalake.Contracts.Models.Tags;
using Datalake.Contracts.Requests;
using Datalake.Gateway.Host.Proxy.Services;
using Datalake.Shared.Hosting.AbstractControllers.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Gateway.Host.Proxy.Controllers.Data;

/// <inheritdoc cref="DataTagsControllerBase" />
public class DataTagsController(DataReverseProxyService proxyService) : DataTagsControllerBase
{
	/// <inheritdoc />
	public override Task<ActionResult<List<TagStatusInfo>>> GetStatusAsync(
		[BindRequired, FromBody] TagMetricRequest request,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<List<TagStatusInfo>>(HttpContext, body: request, cancellationToken: ct);

	/// <inheritdoc />
	public override Task<ActionResult<List<TagUsageInfo>>> GetUsageAsync(
		[BindRequired, FromBody] TagMetricRequest request,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<List<TagUsageInfo>>(HttpContext, body: request, cancellationToken: ct);
}
