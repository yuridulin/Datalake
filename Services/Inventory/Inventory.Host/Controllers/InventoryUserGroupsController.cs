using Datalake.Contracts.Models.UserGroups;
using Datalake.Contracts.Requests;
using Datalake.Inventory.Application.Features.UserGroups.Commands.CreateUserGroup;
using Datalake.Inventory.Application.Features.UserGroups.Commands.DeleteUserGroup;
using Datalake.Inventory.Application.Features.UserGroups.Commands.MoveUserGroup;
using Datalake.Inventory.Application.Features.UserGroups.Commands.UpdateUserGroup;
using Datalake.Inventory.Application.Features.UserGroups.Models;
using Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroup;
using Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroups;
using Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroupsTree;
using Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroupWithDetails;
using Datalake.Shared.Hosting.AbstractControllers.Inventory;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Inventory.Host.Controllers;

public class InventoryUserGroupsController(
	IServiceProvider serviceProvider,
	IAuthenticator authenticator) : InventoryUserGroupsControllerBase
{
	public override async Task<ActionResult<Guid>> CreateAsync(
		[BindRequired, FromBody] UserGroupCreateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<ICreateUserGroupHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
			ParentGuid = request.ParentGuid,
			Name = request.Name,
			Description = request.Description,
		}, ct);

		return data;
	}

	public override async Task<ActionResult<List<UserGroupInfo>>> GetAllAsync(
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetUserGroupsHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
		}, ct);

		return data;
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

		return data;
	}

	public override async Task<ActionResult<List<UserGroupTreeInfo>>> GetTreeAsync(
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetUserGroupsTreeHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
		}, ct);

		return data;
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

		return data;
	}

	public override async Task<ActionResult<bool>> UpdateAsync(
		[BindRequired, FromRoute] Guid userGroupGuid,
		[BindRequired, FromBody] UserGroupUpdateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IUpdateUserGroupHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
			Guid = userGroupGuid,
			Name = request.Name,
			Description = request.Description,
			Users = request.Users.Select(x => new UserRelationDto { Guid = x.Guid, AccessType = x.AccessType })
		}, ct);

		return data;
	}

	public override async Task<ActionResult<bool>> MoveAsync(
		[BindRequired, FromRoute] Guid groupGuid,
		[FromQuery] Guid? parentGuid,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IMoveUserGroupHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
			Guid = groupGuid,
			ParentGuid = parentGuid,
		}, ct);

		return data;
	}

	public override async Task<ActionResult<bool>> DeleteAsync(
		[BindRequired, FromRoute] Guid userGroupGuid,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IDeleteUserGroupHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
			Guid = userGroupGuid,
		}, ct);

		return data;
	}
}