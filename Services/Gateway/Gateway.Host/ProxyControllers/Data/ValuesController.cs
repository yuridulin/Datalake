using Datalake.Contracts.Public.Models.Data.Values;
using Datalake.Gateway.Host.Services;
using Datalake.Shared.Hosting.Controllers.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Gateway.Host.ProxyControllers.Data;

public class ValuesController(DataReverseProxyService proxyService) : BaseValuesController
{
	public override Task<ActionResult<TestCase>> GetAsync(CancellationToken ct = default)
		=> proxyService.ProxyStreamAsync<TestCase>(HttpContext, cancellationToken: ct);

	public override Task<ActionResult<IEnumerable<ValuesResponse>>> GetAsync(
		[BindRequired, FromBody] IEnumerable<ValuesRequest> requests,
		CancellationToken ct = default)
			=> proxyService.ProxyStreamAsync<IEnumerable<ValuesResponse>>(HttpContext, body: requests, cancellationToken: ct);

	public override Task<ActionResult<IEnumerable<ValuesTagResponse>>> WriteAsync(
		[BindRequired, FromBody] IEnumerable<ValueWriteRequest> requests,
		CancellationToken ct = default)
			=> proxyService.ProxyStreamAsync<IEnumerable<ValuesTagResponse>>(HttpContext, body: requests, cancellationToken: ct);
}
