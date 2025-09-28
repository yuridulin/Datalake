using Datalake.Inventory;
using Datalake.PublicApi.Controllers;
using Datalake.PublicApi.Models.UserGroups;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Datalake.InventoryService.Api.Services;
using Datalake.InventoryService.Application.Features.UserGroups;

namespace Datalake.InventoryService.Api.Controllers;

/// <inheritdoc />
public class UserGroupsController(
	InventoryEfContext db,
	AuthenticationService authenticator,
	UserGroupsMemoryRepository userGroupsRepository) : UserGroupsControllerBase
{
	/// <inheritdoc />
	public override async Task<ActionResult<UserGroupInfo>> CreateAsync(
		[BindRequired, FromBody] UserGroupCreateRequest request)
	{
		var user = authenticator.Authenticate(HttpContext);

		return await userGroupsRepository.CreateAsync(db, user, request);
	}

	/// <inheritdoc />
	public override async Task<ActionResult<UserGroupInfo[]>> GetAllAsync()
	{
		var user = authenticator.Authenticate(HttpContext);

		return await Task.FromResult(userGroupsRepository.GetAll(user));
	}

	/// <inheritdoc />
	public override async Task<ActionResult<UserGroupInfo>> GetAsync(
		[BindRequired, FromRoute] Guid groupGuid)
	{
		var user = authenticator.Authenticate(HttpContext);

		return await Task.FromResult(userGroupsRepository.Get(user, groupGuid));
	}

	/// <inheritdoc />
	public override async Task<ActionResult<UserGroupTreeInfo[]>> GetTreeAsync()
	{
		var user = authenticator.Authenticate(HttpContext);

		return await Task.FromResult(userGroupsRepository.GetAllAsTree(user));
	}

	/// <inheritdoc />
	public override async Task<ActionResult<UserGroupDetailedInfo>> GetWithDetailsAsync(
		[BindRequired, FromRoute] Guid groupGuid)
	{
		var user = authenticator.Authenticate(HttpContext);

		return await Task.FromResult(userGroupsRepository.GetWithDetails(user, groupGuid));
	}

	/// <inheritdoc />
	public override async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] Guid groupGuid,
		[BindRequired, FromBody] UserGroupUpdateRequest request)
	{
		var user = authenticator.Authenticate(HttpContext);

		await userGroupsRepository.UpdateAsync(db, user, groupGuid, request);

		return await Task.FromResult(NoContent());
	}

	/// <inheritdoc />
	public override async Task<ActionResult> MoveAsync(
		[BindRequired, FromRoute] Guid groupGuid,
		[FromQuery] Guid? parentGuid)
	{
		var user = authenticator.Authenticate(HttpContext);

		await userGroupsRepository.MoveAsync(db, user, groupGuid, parentGuid);

		return NoContent();
	}

	/// <inheritdoc />
	public override async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] Guid groupGuid)
	{
		var user = authenticator.Authenticate(HttpContext);

		await userGroupsRepository.DeleteAsync(db, user, groupGuid);

		return NoContent();
	}
}