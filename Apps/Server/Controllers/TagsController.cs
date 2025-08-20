using Datalake.Database;
using Datalake.Database.InMemory.Repositories;
using Datalake.PublicApi.Controllers;
using Datalake.PublicApi.Models.Tags;
using Datalake.Server.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;

namespace Datalake.Server.Controllers;

/// <inheritdoc />
public class TagsController(
	DatalakeContext db,
	AuthenticationService authenticator,
	TagsMemoryRepository tagsRepository) : TagsControllerBase
{
	/// <inheritdoc />
	public override async Task<ActionResult<TagInfo>> CreateAsync(
		[BindRequired, FromBody] TagCreateRequest tagCreateRequest)
	{
		var user = authenticator.Authenticate(HttpContext);

		return await tagsRepository.CreateAsync(db, user, tagCreateRequest);
	}

	/// <inheritdoc />
	public override async Task<ActionResult<TagFullInfo>> GetAsync(int id)
	{
		var user = authenticator.Authenticate(HttpContext);

		return await Task.FromResult(tagsRepository.Get(user, id));
	}

	/// <inheritdoc />
	public override async Task<ActionResult<TagInfo[]>> GetAllAsync(
		[FromQuery] int? sourceId,
		[FromQuery] int[]? id,
		[FromQuery] string[]? names,
		[FromQuery] Guid[]? guids)
	{
		var user = authenticator.Authenticate(HttpContext);

		return await Task.FromResult(tagsRepository.GetAll(user, sourceId, id, names, guids));
	}

	/// <inheritdoc />
	public override async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int id,
		[BindRequired, FromBody] TagUpdateRequest tag)
	{
		var user = authenticator.Authenticate(HttpContext);

		await tagsRepository.UpdateAsync(db, user, id, tag);

		return NoContent();
	}

	/// <inheritdoc />
	public override async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int id)
	{
		var user = authenticator.Authenticate(HttpContext);

		await tagsRepository.DeleteAsync(db, user, id);

		return NoContent();
	}
}