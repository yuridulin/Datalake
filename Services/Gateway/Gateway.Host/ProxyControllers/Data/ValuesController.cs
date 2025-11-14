using Datalake.Contracts.Models.Data.Values;
using Datalake.Gateway.Host.Services;
using Datalake.Shared.Hosting.Controllers.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Gateway.Host.ProxyControllers.Data;

public class ValuesController(DataReverseProxyService proxyService) : BaseValuesController
{
	public override Task<ActionResult<IEnumerable<ValuesResponse>>> GetAsync(
		[BindRequired, FromBody] IEnumerable<ValuesRequest> requests,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<IEnumerable<ValuesResponse>>(HttpContext, body: requests, cancellationToken: ct);

	public override Task<ActionResult<IEnumerable<ValuesTagResponse>>> WriteAsync(
		[BindRequired, FromBody] IEnumerable<ValueWriteRequest> requests,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<IEnumerable<ValuesTagResponse>>(HttpContext, body: requests, cancellationToken: ct);
}
