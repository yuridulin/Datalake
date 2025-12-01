using Datalake.Contracts.Models.UserGroups;
using Datalake.Contracts.Requests;
using Datalake.Gateway.Host.Proxy.Services;
using Datalake.Shared.Hosting.AbstractControllers.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Gateway.Host.Proxy.Controllers.Inventory;

/// <inheritdoc cref="InventoryUserGroupsControllerBase" />
public class InventoryUserGroupsController(InventoryReverseProxyService proxyService) : InventoryUserGroupsControllerBase
{
	/// <inheritdoc />
	public override Task<ActionResult<Guid>> CreateAsync(
		[BindRequired, FromBody] UserGroupCreateRequest request,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<Guid>(HttpContext, request, ct);

	/// <inheritdoc />
	public override Task<ActionResult<List<UserGroupInfo>>> GetAllAsync(
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<List<UserGroupInfo>>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<List<UserGroupTreeInfo>>> GetTreeAsync(
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<List<UserGroupTreeInfo>>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<UserGroupInfo>> GetAsync(
		[BindRequired, FromRoute] Guid userGroupGuid,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<UserGroupInfo>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<UserGroupDetailedInfo>> GetWithDetailsAsync(
		[BindRequired, FromRoute] Guid userGroupGuid,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<UserGroupDetailedInfo>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<bool>> MoveAsync(
		[BindRequired, FromRoute] Guid groupGuid,
		[FromQuery] Guid? parentGuid,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<bool>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<bool>> UpdateAsync(
		[BindRequired, FromRoute] Guid userGroupGuid,
		[BindRequired, FromBody] UserGroupUpdateRequest request,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<bool>(HttpContext, request, ct);

	/// <inheritdoc />
	public override Task<ActionResult<bool>> DeleteAsync(
		[BindRequired, FromRoute] Guid userGroupGuid,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<bool>(HttpContext, ct);
}