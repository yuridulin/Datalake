using Datalake.Contracts.Models.AccessRules;
using Datalake.Domain.ValueObjects;
using Datalake.Gateway.Host.Services;
using Datalake.Shared.Hosting.Controllers.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Gateway.Host.ProxyControllers.Inventory;

/// <inheritdoc cref="InventoryAccessControllerBase" />
public class InventoryAccessController(InventoryReverseProxyService proxyService) : InventoryAccessControllerBase
{
	/// <inheritdoc />
	public override Task<ActionResult<AccessRightsInfo[]>> GetAsync(
		[FromQuery(Name = "user")] Guid? userGuid = null,
		[FromQuery(Name = "userGroup")] Guid? userGroupGuid = null,
		[FromQuery(Name = "source")] int? sourceId = null,
		[FromQuery(Name = "block")] int? blockId = null,
		[FromQuery(Name = "tag")] int? tagId = null,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<AccessRightsInfo[]>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<IDictionary<Guid, UserAccessValue>>> GetCalculatedAccessAsync(
		[FromBody] IEnumerable<Guid>? guids,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<IDictionary<Guid, UserAccessValue>>(HttpContext, guids, ct);

	/// <inheritdoc />
	public override Task<ActionResult> SetBlockRulesAsync(
		[FromRoute] int blockId,
		[FromBody] AccessRuleForObjectRequest[] requests,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync(HttpContext, requests, ct);

	/// <inheritdoc />
	public override Task<ActionResult> SetSourceRulesAsync(
		[FromRoute] int sourceId,
		[FromBody] AccessRuleForObjectRequest[] requests,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync(HttpContext, requests, ct);

	/// <inheritdoc />
	public override Task<ActionResult> SetTagRulesAsync(
		[FromRoute] int tagId,
		[FromBody] AccessRuleForObjectRequest[] requests,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync(HttpContext, requests, ct);

	/// <inheritdoc />
	public override Task<ActionResult> SetUserGroupRulesAsync(
		[FromRoute] Guid userGroupGuid,
		[FromBody] AccessRuleForActorRequest[] requests,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync(HttpContext, requests, ct);

	/// <inheritdoc />
	public override Task<ActionResult> SetUserRulesAsync(
		[FromRoute] Guid userGuid,
		[FromBody] AccessRuleForActorRequest[] requests,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync(HttpContext, requests, ct);
}
