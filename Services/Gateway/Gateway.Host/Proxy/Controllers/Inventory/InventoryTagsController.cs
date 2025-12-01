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
	public override Task<ActionResult<TagSimpleInfo>> CreateAsync(
		[BindRequired, FromBody] TagCreateRequest request,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<TagSimpleInfo>(HttpContext, request, ct);

	/// <inheritdoc />
	public override Task<ActionResult<TagWithSettingsAndBlocksInfo>> GetWithSettingsAndBlocksAsync(
		[FromRoute] int tagId,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<TagWithSettingsAndBlocksInfo>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<List<TagSimpleInfo>>> GetAllAsync(
		[FromQuery] int? sourceId,
		[FromQuery] int[]? tagsId,
		[FromQuery] Guid[]? tagsGuid,
		[FromQuery] TagType? type,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<List<TagSimpleInfo>>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<List<TagWithSettingsInfo>>> GetAllWithSettingsAsync(
		[FromQuery] int? sourceId,
		[FromQuery] int[]? tagsId,
		[FromQuery] Guid[]? tagsGuid,
		[FromQuery] TagType? type,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<List<TagWithSettingsInfo>>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<bool>> UpdateAsync(
		[BindRequired, FromRoute] int tagId,
		[BindRequired, FromBody] TagUpdateRequest request,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<bool>(HttpContext, request, ct);

	/// <inheritdoc />
	public override Task<ActionResult<bool>> DeleteAsync(
		[BindRequired, FromRoute] int tagId,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<bool>(HttpContext, ct);
}
