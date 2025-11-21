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
	public override Task<ActionResult<IDictionary<int, string>>> GetStatusAsync(
		[BindRequired, FromBody] TagMetricRequest request,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<IDictionary<int, string>>(HttpContext, body: request, cancellationToken: ct);

	/// <inheritdoc />
	public override Task<ActionResult<IDictionary<int, IDictionary<string, DateTime>>>> GetUsageAsync(
		[BindRequired, FromBody] TagMetricRequest request,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<IDictionary<int, IDictionary<string, DateTime>>>(HttpContext, body: request, cancellationToken: ct);
}
