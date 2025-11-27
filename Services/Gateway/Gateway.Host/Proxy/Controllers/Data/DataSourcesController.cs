using Datalake.Contracts.Models.Sources;
using Datalake.Gateway.Host.Proxy.Services;
using Datalake.Shared.Hosting.AbstractControllers.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Gateway.Host.Proxy.Controllers.Data;

/// <inheritdoc cref="DataSourcesControllerBase" />
public class DataSourcesController(DataReverseProxyService proxyService) : DataSourcesControllerBase
{
	/// <inheritdoc />
	public override Task<ActionResult<SourceActivityInfo[]>> GetActivityAsync(
		[BindRequired, FromBody] int[] sourcesId,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<SourceActivityInfo[]>(HttpContext, body: sourcesId, cancellationToken: ct);

	/// <inheritdoc />
	public override Task<ActionResult<List<SourceItemInfo>>> GetItemsAsync(
		[BindRequired, FromRoute] int sourceId,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<List<SourceItemInfo>>(HttpContext, cancellationToken: ct);
}
