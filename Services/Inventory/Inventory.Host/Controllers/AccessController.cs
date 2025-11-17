using Datalake.Contracts.Models.AccessRules;
using Datalake.Domain.ValueObjects;
using Datalake.Inventory.Application.Features.AccessRules.Commands.ChangeBlockRules;
using Datalake.Inventory.Application.Features.AccessRules.Commands.ChangeSourceRules;
using Datalake.Inventory.Application.Features.AccessRules.Commands.ChangeTagRules;
using Datalake.Inventory.Application.Features.AccessRules.Commands.ChangeUserGroupRules;
using Datalake.Inventory.Application.Features.AccessRules.Commands.ChangeUserRules;
using Datalake.Inventory.Application.Features.AccessRules.Models;
using Datalake.Inventory.Application.Features.AccessRules.Queries.GetAccessRules;
using Datalake.Inventory.Application.Features.CalculatedAccessRules.Queries.GetCalculatedAccessRules;
using Datalake.Shared.Hosting.Controllers.Inventory;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace Datalake.Inventory.Host.Controllers;

public class AccessController(
	IServiceProvider serviceProvider,
	IAuthenticator authenticator) : InventoryAccessControllerBase
{
	public override async Task<ActionResult<AccessRightsInfo[]>> GetAsync(
		[FromQuery(Name = "user")] Guid? userGuid = null,
		[FromQuery(Name = "userGroup")] Guid? userGroupGuid = null,
		[FromQuery(Name = "source")] int? sourceId = null,
		[FromQuery(Name = "block")] int? blockId = null,
		[FromQuery(Name = "tag")] int? tagId = null,
		CancellationToken ct = default)
	{
		_ = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetAccessRulesHandler>();
		var data = await handler.HandleAsync(new()
		{
			BlockId = blockId,
			SourceId = sourceId,
			TagId = tagId,
			UserGroupGuid = userGroupGuid,
			UserGuid = userGuid,
		}, ct);

		return Ok(data);
	}

	public override async Task<ActionResult<IDictionary<Guid, UserAccessValue>>> GetCalculatedAccessAsync(
		[FromBody, Optional] IEnumerable<Guid>? guids,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetCalculatedAccessRulesHandler>();
		var data = await handler.HandleAsync(new() { User = user, Guids = guids }, ct);

		return Ok(data);
	}

	public override async Task<ActionResult> SetUserRulesAsync(
		[FromRoute] Guid userGuid,
		[FromBody] AccessRuleForActorRequest[] requests,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IChangeUserRulesHandler>();
		await handler.HandleAsync(new()
		{
			User = user,
			UserGuid = userGuid,
			Rules = requests.Select(x => new ActorRuleDto()
			{
				Type = x.AccessType,
				SourceId = x.SourceId,
				BlockId = x.BlockId,
				TagId = x.TagId,
			})
		}, ct);

		return NoContent();
	}

	public override async Task<ActionResult> SetUserGroupRulesAsync(
		[FromRoute] Guid userGroupGuid,
		[FromBody] AccessRuleForActorRequest[] requests,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IChangeUserGroupRulesHandler>();
		await handler.HandleAsync(new()
		{
			User = user,
			UserGroupGuid = userGroupGuid,
			Rules = requests.Select(x => new ActorRuleDto()
			{
				Type = x.AccessType,
				SourceId = x.SourceId,
				BlockId = x.BlockId,
				TagId = x.TagId,
			})
		}, ct);

		return NoContent();
	}

	public override async Task<ActionResult> SetSourceRulesAsync(
		[FromRoute] int sourceId,
		[FromBody] AccessRuleForObjectRequest[] requests,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IChangeSourceRulesHandler>();
		await handler.HandleAsync(new(
			user,
			sourceId,
			requests.Select(x => new ObjectRuleDto(
				x.AccessType,
				UserGuid: x.UserGuid,
				UserGroupGuid: x.UserGroupGuid))), ct);

		return NoContent();
	}

	public override async Task<ActionResult> SetBlockRulesAsync(
		[FromRoute] int blockId,
		[FromBody] AccessRuleForObjectRequest[] requests,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IChangeBlockRulesHandler>();
		await handler.HandleAsync(new(
			user,
			blockId,
			requests.Select(x => new ObjectRuleDto(
				x.AccessType,
				UserGuid: x.UserGuid,
				UserGroupGuid: x.UserGroupGuid))), ct);

		return NoContent();
	}

	public override async Task<ActionResult> SetTagRulesAsync(
		[FromRoute] int tagId,
		[FromBody] AccessRuleForObjectRequest[] requests,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IChangeTagRulesHandler>();
		await handler.HandleAsync(new(
			user,
			tagId,
			requests.Select(x => new ObjectRuleDto(
				x.AccessType,
				UserGuid: x.UserGuid,
				UserGroupGuid: x.UserGroupGuid))), ct);

		return NoContent();
	}
}
