using Datalake.Gateway.Host.Services;
using Datalake.Shared.Hosting.Controllers.Data;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Gateway.Host.ProxyControllers.Data;

/// <inheritdoc cref="DataSourcesControllerBase" />
public class DataSystemController(DataReverseProxyService proxyService) : DataSystemControllerBase
{
	/// <inheritdoc />
	public override Task<ActionResult> RestartCollectionAsync(CancellationToken ct = default)
			=> proxyService.ProxyAsync(HttpContext, cancellationToken: ct);
}
