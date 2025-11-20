using Datalake.Gateway.Host.Proxy.Services;
using Datalake.Shared.Hosting.AbstractControllers.Data;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Gateway.Host.Proxy.Controllers.Data;

/// <inheritdoc cref="DataSourcesControllerBase" />
public class DataSystemController(DataReverseProxyService proxyService) : DataSystemControllerBase
{
	/// <inheritdoc />
	public override Task<ActionResult> RestartCollectionAsync(CancellationToken ct = default)
			=> proxyService.ProxyAsync(HttpContext, cancellationToken: ct);
}
