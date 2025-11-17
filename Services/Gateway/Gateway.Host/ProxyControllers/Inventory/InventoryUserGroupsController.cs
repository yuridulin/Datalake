using Datalake.Contracts.Models.UserGroups;
using Datalake.Gateway.Host.Services;
using Datalake.Shared.Hosting.Controllers.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Gateway.Host.ProxyControllers.Inventory;

/// <inheritdoc cref="InventoryUserGroupsControllerBase" />
public class InventoryUserGroupsController(InventoryReverseProxyService proxyService) : InventoryUserGroupsControllerBase
{
	/// <inheritdoc />
	public override Task<ActionResult<Guid>> CreateAsync(
		[BindRequired, FromBody] UserGroupCreateRequest request,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<Guid>(HttpContext, request, ct);

	/// <inheritdoc />
	public override Task<ActionResult<IEnumerable<UserGroupInfo>>> GetAllAsync(
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<IEnumerable<UserGroupInfo>>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<UserGroupTreeInfo[]>> GetTreeAsync(
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<UserGroupTreeInfo[]>(HttpContext, ct);

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
	public override Task<ActionResult> MoveAsync(
		[BindRequired, FromRoute] Guid groupGuid,
		[FromQuery] Guid? parentGuid,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] Guid userGroupGuid,
		[BindRequired, FromBody] UserGroupUpdateRequest request,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync(HttpContext, request, ct);

	/// <inheritdoc />
	public override Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] Guid userGroupGuid,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync(HttpContext, ct);
}