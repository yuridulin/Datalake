using DatalakeApiClasses.Exceptions;
using DatalakeApiClasses.Models.UserGroups;
using DatalakeDatabase.Repositories;
using DatalakeServer.ApiControllers.Base;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DatalakeServer.ApiControllers;

[Route("api/[controller]")]
[ApiController]
public class UserGroupsController(UserGroupsRepository userGroupsRepository) : ApiControllerBase
{
	[HttpPost]
	public async Task<ActionResult<Guid>> CreateAsync(
		[BindRequired, FromBody] CreateUserGroupRequest request)
	{
		var user = Authenticate();

		return await userGroupsRepository.CreateAsync(user, request);
	}

	[HttpGet]
	public async Task<ActionResult<UserGroupInfo[]>> ReadAsync()
	{
		return await userGroupsRepository.GetInfo()
			.ToArrayAsync();
	}

	[HttpGet("{groupGuid}")]
	public async Task<ActionResult<UserGroupInfo>> ReadAsync(
		[BindRequired, FromRoute] Guid groupGuid)
	{
		return await userGroupsRepository.GetInfo()
			.Where(x => x.UserGroupGuid == groupGuid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"группа {groupGuid}");
	}

	[HttpGet("{groupGuid}/detailed")]
	public async Task<ActionResult<UserGroupDetailedInfo>> ReadWithDetailsAsync(
		[BindRequired, FromRoute] Guid groupGuid)
	{
		return await userGroupsRepository.GetWithChildsAndUsers()
			.Where(x => x.UserGroupGuid == groupGuid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"группа {groupGuid}");
	}

	[HttpPut("{groupGuid}")]
	public async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] Guid groupGuid,
		[BindRequired, FromBody] UpdateUserGroupRequest request)
	{
		var user = Authenticate();

		await userGroupsRepository.UpdateAsync(user, groupGuid, request);

		return NoContent();
	}

	[HttpDelete("{groupGuid}")]
	public async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] Guid groupGuid)
	{
		var user = Authenticate();

		await userGroupsRepository.DeleteAsync(user, groupGuid);

		return NoContent();
	}
}
