using Datalake.Contracts.Models.Data.Values;
using Datalake.Gateway.Host.Proxy.Services;
using Datalake.Shared.Hosting.AbstractControllers.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Gateway.Host.Proxy.Controllers.Data;

/// <inheritdoc cref="DataValuesControllerBase" />
public class DataValuesController(DataReverseProxyService proxyService) : DataValuesControllerBase
{
	/// <inheritdoc />
	public override Task<ActionResult<List<ValuesResponse>>> GetAsync(
		[BindRequired, FromBody] IEnumerable<ValuesRequest> requests,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<List<ValuesResponse>>(HttpContext, body: requests, cancellationToken: ct);

	/// <inheritdoc />
	public override Task<ActionResult<List<ValuesTagResponse>>> WriteAsync(
		[BindRequired, FromBody] IEnumerable<ValueWriteRequest> requests,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<List<ValuesTagResponse>>(HttpContext, body: requests, cancellationToken: ct);
}
