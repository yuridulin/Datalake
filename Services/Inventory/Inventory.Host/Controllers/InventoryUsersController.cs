using Datalake.Contracts.Models.Users;
using Datalake.Contracts.Requests;
using Datalake.Inventory.Application.Features.Users.Commands.CreateUser;
using Datalake.Inventory.Application.Features.Users.Commands.DeleteUser;
using Datalake.Inventory.Application.Features.Users.Commands.UpdateUser;
using Datalake.Inventory.Application.Features.Users.Queries.GetUsers;
using Datalake.Inventory.Application.Features.Users.Queries.GetUserWithDetails;
using Datalake.Shared.Hosting.AbstractControllers.Inventory;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Inventory.Host.Controllers;

public class InventoryUsersController(
	IServiceProvider serviceProvider,
	IAuthenticator authenticator) : InventoryUsersControllerBase
{
	public override async Task<ActionResult<Guid>> CreateAsync(
		[BindRequired, FromBody] UserCreateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<ICreateUserHandler>();
		var result = await handler.HandleAsync(new()
		{
			User = user,
			Type = request.Type,
			EnergoIdGuid = request.EnergoIdGuid,
			Login = request.Login,
			Password = request.Password,
			FullName = request.FullName,
			Email = request.Email,
			AccessType = request.AccessType,
		}, ct);

		return Ok(result);
	}

	public override async Task<ActionResult<IEnumerable<UserInfo>>> GetAsync(
		[FromQuery] Guid? userGuid,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetUsersHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
			UserGuid = userGuid,
		}, ct);

		return Ok(data);
	}

	public override async Task<ActionResult<UserWithGroupsInfo>> GetWithDetailsAsync(
		[BindRequired, FromRoute] Guid userGuid,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetUserWithDetailsHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
			Guid = userGuid,
		}, ct);

		return Ok(data);
	}

	public override async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] Guid userGuid,
		[BindRequired, FromBody] UserUpdateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IUpdateUserHandler>();
		await handler.HandleAsync(new()
		{
			User = user,
			Guid = userGuid,
			Login = request.Login,
			Password = request.Password,
			FullName = request.FullName,
			Email = request.Email,
			AccessType = request.AccessType,
		}, ct);

		return NoContent();
	}

	public override async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] Guid userGuid,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IDeleteUserHandler>();
		await handler.HandleAsync(new()
		{
			User = user,
			Guid = userGuid,
		}, ct);

		return NoContent();
	}
}