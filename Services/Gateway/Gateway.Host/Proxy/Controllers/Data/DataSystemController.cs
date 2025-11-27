using Datalake.Gateway.Host.Proxy.Services;
using Datalake.Shared.Hosting.AbstractControllers.Data;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Gateway.Host.Proxy.Controllers.Data;

/// <inheritdoc cref="DataSourcesControllerBase" />
public class DataSystemController(DataReverseProxyService proxyService) : DataSystemControllerBase
{
	/// <inheritdoc />
	public override Task<ActionResult<bool>> RestartCollectionAsync(CancellationToken ct = default)
		=> proxyService.ProxyAsync<bool>(HttpContext, cancellationToken: ct);
}
