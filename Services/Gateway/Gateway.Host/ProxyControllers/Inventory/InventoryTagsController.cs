using Datalake.Contracts.Models.Tags;
using Datalake.Gateway.Host.Services;
using Datalake.Shared.Hosting.Controllers.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Gateway.Host.ProxyControllers.Inventory;

/// <inheritdoc />
public class InventoryTagsController(InventoryReverseProxyService proxyService) : InventoryTagsControllerBase
{
	/// <inheritdoc />
	public override Task<ActionResult<TagInfo>> CreateAsync(
		[BindRequired, FromBody] TagCreateRequest request,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<TagInfo>(HttpContext, request, ct);

	/// <inheritdoc />
	public override Task<ActionResult<TagFullInfo>> GetAsync(
		[FromRoute] int tagId,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<TagFullInfo>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<TagInfo[]>> GetAllAsync(
		[FromQuery] int? sourceId,
		[FromQuery] int[]? tagsId,
		[FromQuery] Guid[]? tagsGuid,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<TagInfo[]>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int tagId,
		[BindRequired, FromBody] TagUpdateRequest request,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync(HttpContext, request, ct);

	/// <inheritdoc />
	public override Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int tagId,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync(HttpContext, ct);
}
