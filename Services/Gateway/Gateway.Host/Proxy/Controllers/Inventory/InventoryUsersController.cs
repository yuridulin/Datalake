using Datalake.Contracts.Models.Users;
using Datalake.Contracts.Requests;
using Datalake.Gateway.Host.Proxy.Services;
using Datalake.Shared.Hosting.AbstractControllers.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Gateway.Host.Proxy.Controllers.Inventory;

/// <inheritdoc cref="InventoryUsersControllerBase" />
public class InventoryUsersController(InventoryReverseProxyService proxyService) : InventoryUsersControllerBase
{
	/// <inheritdoc />
	public override Task<ActionResult<Guid>> CreateAsync(
		[BindRequired, FromBody] UserCreateRequest request,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<Guid>(HttpContext, request, ct);

	/// <inheritdoc />
	public override Task<ActionResult<List<UserInfo>>> GetAsync(
		[FromQuery] Guid? userGuid,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<List<UserInfo>>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<UserWithGroupsInfo>> GetWithDetailsAsync(
		[BindRequired, FromRoute] Guid userGuid,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<UserWithGroupsInfo>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<bool>> UpdateAsync(
		[BindRequired, FromRoute] Guid userGuid,
		[BindRequired, FromBody] UserUpdateRequest request,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<bool>(HttpContext, request, ct);

	/// <inheritdoc />
	public override Task<ActionResult<bool>> DeleteAsync(
		[BindRequired, FromRoute] Guid userGuid,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<bool>(HttpContext, ct);
}
