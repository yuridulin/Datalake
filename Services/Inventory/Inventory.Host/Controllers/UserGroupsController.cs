using Datalake.Contracts.Models.UserGroups;
using Datalake.Inventory.Application.Features.UserGroups.Commands.CreateUserGroup;
using Datalake.Inventory.Application.Features.UserGroups.Commands.DeleteUserGroup;
using Datalake.Inventory.Application.Features.UserGroups.Commands.MoveUserGroup;
using Datalake.Inventory.Application.Features.UserGroups.Commands.UpdateUserGroup;
using Datalake.Inventory.Application.Features.UserGroups.Models;
using Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroup;
using Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroups;
using Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroupWithDetails;
using Datalake.Shared.Hosting.Controllers.Inventory;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Inventory.Host.Controllers;

public class UserGroupsController(
	IServiceProvider serviceProvider,
	IAuthenticator authenticator) : InventoryUserGroupsControllerBase
{
	public override async Task<ActionResult<Guid>> CreateAsync(
		[BindRequired, FromBody] UserGroupCreateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<ICreateUserGroupHandler>();
		var result = await handler.HandleAsync(new()
		{
			User = user,
			ParentGuid = request.ParentGuid,
			Name = request.Name,
			Description = request.Description,
		}, ct);

		return Ok(result);
	}

	public override async Task<ActionResult<IEnumerable<UserGroupInfo>>> GetAllAsync(
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetUserGroupsHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
		}, ct);

		return Ok(data);
	}

	public override async Task<ActionResult<UserGroupInfo>> GetAsync(
		[BindRequired, FromRoute] Guid userGroupGuid,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetUserGroupHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
			Guid = userGroupGuid,
		}, ct);

		return Ok(data);
	}

	public override async Task<ActionResult<UserGroupTreeInfo[]>> GetTreeAsync(
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetUserGroupsHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
		}, ct);

		return Ok(data);
	}

	public override async Task<ActionResult<UserGroupDetailedInfo>> GetWithDetailsAsync(
		[BindRequired, FromRoute] Guid userGroupGuid,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetUserGroupWithDetailsHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
			Guid = userGroupGuid,
		}, ct);

		return Ok(data);
	}

	public override async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] Guid userGroupGuid,
		[BindRequired, FromBody] UserGroupUpdateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IUpdateUserGroupHandler>();
		await handler.HandleAsync(new()
		{
			User = user,
			Guid = userGroupGuid,
			Name = request.Name,
			Description = request.Description,
			Users = request.Users.Select(x => new UserRelationDto { Guid = x.Guid, AccessType = x.AccessType })
		}, ct);

		return NoContent();
	}

	public override async Task<ActionResult> MoveAsync(
		[BindRequired, FromRoute] Guid groupGuid,
		[FromQuery] Guid? parentGuid,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IMoveUserGroupHandler>();
		await handler.HandleAsync(new()
		{
			User = user,
			Guid = groupGuid,
			ParentGuid = parentGuid,
		}, ct);

		return NoContent();
	}

	public override async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] Guid userGroupGuid,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IDeleteUserGroupHandler>();
		await handler.HandleAsync(new()
		{
			User = user,
			Guid = userGroupGuid,
		}, ct);

		return NoContent();
	}
}