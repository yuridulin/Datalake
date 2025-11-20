using Datalake.Contracts.Models.Users;
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
	public override Task<ActionResult<IEnumerable<UserInfo>>> GetAsync(
		[FromQuery] Guid? userGuid,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<IEnumerable<UserInfo>>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<UserInfo>> GetWithDetailsAsync(
		[BindRequired, FromRoute] Guid userGuid,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<UserInfo>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] Guid userGuid,
		[BindRequired, FromBody] UserUpdateRequest request,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync(HttpContext, request, ct);

	/// <inheritdoc />
	public override Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] Guid userGuid,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync(HttpContext, ct);
}
