using Datalake.Contracts.Models.AccessRules;
using Datalake.Contracts.Requests;
using Datalake.Domain.ValueObjects;
using Datalake.Gateway.Host.Proxy.Services;
using Datalake.Shared.Hosting.AbstractControllers.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Gateway.Host.Proxy.Controllers.Inventory;

/// <inheritdoc cref="InventoryAccessControllerBase" />
public class InventoryAccessController(InventoryReverseProxyService proxyService) : InventoryAccessControllerBase
{
	/// <inheritdoc />
	public override Task<ActionResult<List<AccessRightsInfo>>> GetAsync(
		[FromQuery(Name = "user")] Guid? userGuid = null,
		[FromQuery(Name = "userGroup")] Guid? userGroupGuid = null,
		[FromQuery(Name = "source")] int? sourceId = null,
		[FromQuery(Name = "block")] int? blockId = null,
		[FromQuery(Name = "tag")] int? tagId = null,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<List<AccessRightsInfo>>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<Dictionary<Guid, UserAccessValue>>> GetCalculatedAccessAsync(
		[FromBody] IEnumerable<Guid>? guids,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<Dictionary<Guid, UserAccessValue>>(HttpContext, guids, ct);

	/// <inheritdoc />
	public override Task<ActionResult<bool>> SetBlockRulesAsync(
		[FromRoute] int blockId,
		[FromBody] AccessRuleForObjectRequest[] requests,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<bool>(HttpContext, requests, ct);

	/// <inheritdoc />
	public override Task<ActionResult<bool>> SetSourceRulesAsync(
		[FromRoute] int sourceId,
		[FromBody] AccessRuleForObjectRequest[] requests,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<bool>(HttpContext, requests, ct);

	/// <inheritdoc />
	public override Task<ActionResult<bool>> SetTagRulesAsync(
		[FromRoute] int tagId,
		[FromBody] AccessRuleForObjectRequest[] requests,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<bool>(HttpContext, requests, ct);

	/// <inheritdoc />
	public override Task<ActionResult<bool>> SetUserGroupRulesAsync(
		[FromRoute] Guid userGroupGuid,
		[FromBody] AccessRuleForActorRequest[] requests,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<bool>(HttpContext, requests, ct);

	/// <inheritdoc />
	public override Task<ActionResult<bool>> SetUserRulesAsync(
		[FromRoute] Guid userGuid,
		[FromBody] AccessRuleForActorRequest[] requests,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<bool>(HttpContext, requests, ct);
}
