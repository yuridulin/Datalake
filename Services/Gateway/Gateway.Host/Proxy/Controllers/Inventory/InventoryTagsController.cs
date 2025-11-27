using Datalake.Contracts.Models.Tags;
using Datalake.Contracts.Requests;
using Datalake.Domain.Enums;
using Datalake.Gateway.Host.Proxy.Services;
using Datalake.Shared.Hosting.AbstractControllers.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Gateway.Host.Proxy.Controllers.Inventory;

/// <inheritdoc cref="InventoryTagsControllerBase" />
public class InventoryTagsController(InventoryReverseProxyService proxyService) : InventoryTagsControllerBase
{
	/// <inheritdoc />
	public override Task<ActionResult<int>> CreateAsync(
		[BindRequired, FromBody] TagCreateRequest request,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<int>(HttpContext, request, ct);

	/// <inheritdoc />
	public override Task<ActionResult<TagWithSettingsAndBlocksInfo>> GetWithSettingsAndBlocksAsync(
		[FromRoute] int tagId,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<TagWithSettingsAndBlocksInfo>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<TagSimpleInfo[]>> GetAllAsync(
		[FromQuery] int? sourceId,
		[FromQuery] int[]? tagsId,
		[FromQuery] Guid[]? tagsGuid,
		[FromQuery] TagType? type,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<TagSimpleInfo[]>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<TagWithSettingsInfo[]>> GetAllWithSettingsAsync(
		[FromQuery] int? sourceId,
		[FromQuery] int[]? tagsId,
		[FromQuery] Guid[]? tagsGuid,
		[FromQuery] TagType? type,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<TagWithSettingsInfo[]>(HttpContext, ct);

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
