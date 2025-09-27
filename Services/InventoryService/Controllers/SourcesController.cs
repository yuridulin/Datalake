using Datalake.Inventory;
using Datalake.PublicApi.Controllers;
using Datalake.PublicApi.Models.Sources;
using Datalake.InventoryService.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Datalake.InventoryService.InMemory.Repositories;
using Datalake.InventoryService.Database.Repositories;

namespace Datalake.InventoryService.Controllers;

/// <inheritdoc />
public class SourcesController(
	InventoryEfContext db,
	AuthenticationService authenticator,
	SourcesMemoryRepository sourcesRepository) : SourcesControllerBase
{
	/// <inheritdoc />
	public override async Task<ActionResult<SourceInfo>> CreateEmptyAsync()
	{
		var user = authenticator.Authenticate(HttpContext);

		var info = await sourcesRepository.CreateAsync(db, user);

		return info;
	}

	/// <inheritdoc />
	public override async Task<ActionResult<SourceInfo>> CreateAsync(
		[BindRequired, FromBody] SourceInfo source)
	{
		var user = authenticator.Authenticate(HttpContext);

		var info = await sourcesRepository.CreateAsync(db, user, source);

		return info;
	}

	/// <inheritdoc />
	public override async Task<ActionResult<SourceInfo>> GetAsync(
		[BindRequired, FromRoute] int id)
	{
		var user = authenticator.Authenticate(HttpContext);

		return await Task.FromResult(sourcesRepository.Get(user, id));
	}

	/// <inheritdoc />
	public override async Task<ActionResult<SourceInfo[]>> GetAllAsync(bool withCustom = false)
	{
		var user = authenticator.Authenticate(HttpContext);

		return await Task.FromResult(sourcesRepository.GetAll(user, withCustom));
	}

	/// <inheritdoc />
	public override async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int id,
		[BindRequired, FromBody] SourceUpdateRequest request)
	{
		var user = authenticator.Authenticate(HttpContext);

		await sourcesRepository.UpdateAsync(db, user, id, request);

		return NoContent();
	}

	/// <inheritdoc />
	public override async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int id)
	{
		var user = authenticator.Authenticate(HttpContext);

		await sourcesRepository.DeleteAsync(db, user, id);

		return NoContent();
	}
}