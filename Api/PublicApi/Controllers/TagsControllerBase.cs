using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Models.Tags;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.PublicApi.Controllers;

[ApiController]
[Route($"{Defaults.ApiRoot}/{ControllerRoute}")]
public abstract class TagsControllerBase : ControllerBase
{
	#region Константы путей

	/// <summary>
	/// Основной путь к контроллеру
	/// </summary>
	public const string ControllerRoute = "tags";

	/// <inheritdoc cref="CreateAsync(TagCreateRequest)" />
	public const string Create = "";

	/// <inheritdoc cref="GetAsync(int)" />
	public const string Get = "{id}";

	/// <inheritdoc cref="GetAllAsync" />
	public const string GetAll = "";

	/// <inheritdoc cref="UpdateAsync(int, TagUpdateRequest)" />
	public const string Update = "{id}";

	/// <inheritdoc cref="DeleteAsync(int)" />
	public const string Delete = "{id}";

	#endregion Константы путей

	#region Методы

	[HttpPost(Create)]
	public abstract Task<ActionResult<TagInfo>> CreateAsync(
		[BindRequired, FromBody] TagCreateRequest tagCreateRequest);

	[HttpGet(Get)]
	public abstract Task<ActionResult<TagFullInfo>> GetAsync(
			[BindRequired, FromRoute] int id);

	[HttpGet(GetAll)]
	public abstract Task<ActionResult<TagInfo[]>> GetAllAsync(
		[FromQuery] int? sourceId,
		[FromQuery] int[]? id,
		[FromQuery] string[]? names,
		[FromQuery] Guid[]? guids);

	[HttpPut(Update)]
	public abstract Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int id,
		[BindRequired, FromBody] TagUpdateRequest tag);

	[HttpDelete(Delete)]
	public abstract Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int id);

	#endregion Методы
}
