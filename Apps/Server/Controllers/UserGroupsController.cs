using Datalake.Database;
using Datalake.Database.InMemory.Repositories;
using Datalake.PublicApi.Controllers;
using Datalake.PublicApi.Models.UserGroups;
using Datalake.Server.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Server.Controllers;

/// <inheritdoc />
public class UserGroupsController(
	DatalakeContext db,
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
	public override ActionResult<UserGroupInfo[]> GetAll()
	{
		var user = authenticator.Authenticate(HttpContext);

		return userGroupsRepository.GetAll(user);
	}

	/// <inheritdoc />
	public override ActionResult<UserGroupInfo> Get(
		[BindRequired, FromRoute] Guid groupGuid)
	{
		var user = authenticator.Authenticate(HttpContext);

		return userGroupsRepository.Get(user, groupGuid);
	}

	/// <inheritdoc />
	public override ActionResult<UserGroupTreeInfo[]> GetTree()
	{
		var user = authenticator.Authenticate(HttpContext);

		return userGroupsRepository.GetAllAsTree(user);
	}

	/// <inheritdoc />
	public override ActionResult<UserGroupDetailedInfo> GetWithDetails(
		[BindRequired, FromRoute] Guid groupGuid)
	{
		var user = authenticator.Authenticate(HttpContext);

		return userGroupsRepository.GetWithDetails(user, groupGuid);
	}

	/// <inheritdoc />
	public override async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] Guid groupGuid,
		[BindRequired, FromBody] UserGroupUpdateRequest request)
	{
		var user = authenticator.Authenticate(HttpContext);

		await userGroupsRepository.UpdateAsync(db, user, groupGuid, request);

		return NoContent();
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