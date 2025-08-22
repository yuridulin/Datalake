using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Blocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.PublicApi.Controllers;

/// <summary>
/// Взаимодействие с блоками
/// </summary>
[ApiController]
[Route($"{Defaults.ApiRoot}/{ControllerRoute}")]
public abstract class BlocksControllerBase : ControllerBase
{
	#region Константы путей

	/// <summary>
	/// Основной путь к контроллеру
	/// </summary>
	public const string ControllerRoute = "blocks";

	/// <inheritdoc cref="CreateAsync" />
	public const string Create = "";

	/// <inheritdoc cref="CreateEmptyAsync" />
	public const string CreateEmpty = "empty";

	/// <inheritdoc cref="GetAllAsync" />
	public const string GetAll = "";

	/// <inheritdoc cref="GetAsync" />
	public const string Get = "{id}";

	/// <inheritdoc cref="GetTreeAsync" />
	public const string GetTree = "tree";

	/// <inheritdoc cref="UpdateAsync" />
	public const string Update = "{id}";

	/// <inheritdoc cref="MoveAsync" />
	public const string Move = "{id}/move";

	/// <inheritdoc cref="DeleteAsync" />
	public const string Delete = "{id}";

	#endregion Константы путей

	#region Методы

	/// <summary>
	/// <see cref="HttpMethod.Post" />: Создание нового блока на основании переданной информации
	/// </summary>
	/// <param name="blockInfo">Данные о новом блоке</param>
	/// <returns>Идентификатор блока</returns>
	[HttpPost(Create)]
	public abstract Task<ActionResult<BlockWithTagsInfo>> CreateAsync(
		[BindRequired, FromBody] BlockFullInfo blockInfo);

	/// <summary>
	/// <see cref="HttpMethod.Post" />: Создание нового отдельного блока с информацией по умолчанию
	/// </summary>
	/// <param name="parentId">Идентификатор родительского блока</param>
	/// <returns>Идентификатор блока</returns>
	[HttpPost(CreateEmpty)]
	public abstract Task<ActionResult<BlockWithTagsInfo>> CreateEmptyAsync(
		[FromQuery] int? parentId);

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение списка блоков с базовой информацией о них
	/// </summary>
	/// <returns>Список блоков</returns>
	[HttpGet(GetAll)]
	public abstract Task<ActionResult<BlockWithTagsInfo[]>> GetAllAsync();

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение информации о выбранном блоке
	/// </summary>
	/// <param name="id">Идентификатор блока</param>
	/// <returns>Информация о блоке</returns>
	/// <exception cref="NotFoundException">Блок не найден по идентификатору</exception>
	[HttpGet(Get)]
	public abstract Task<ActionResult<BlockFullInfo>> GetAsync(
		[BindRequired, FromRoute] int id);

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение иерархической структуры всех блоков
	/// </summary>
	/// <returns>Список обособленных блоков с вложенными блоками</returns>
	[HttpGet(GetTree)]
	public abstract Task<ActionResult<BlockTreeInfo[]>> GetTreeAsync();

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Изменение блока
	/// </summary>
	/// <param name="id">Идентификатор блока</param>
	/// <param name="block">Новые данные блока</param>
	[HttpPut(Update)]
	public abstract Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int id,
		[BindRequired, FromBody] BlockUpdateRequest block);

	/// <summary>
	/// <see cref="HttpMethod.Post" />: Перемещение блока
	/// </summary>
	/// <param name="id">Идентификатор блока</param>
	/// <param name="parentId">Идентификатор нового родительского блока</param>
	[HttpPost(Move)]
	public abstract Task<ActionResult> MoveAsync(
		[BindRequired, FromRoute] int id,
		[FromQuery] int? parentId);

	/// <summary>
	/// <see cref="HttpMethod.Delete" />: Удаление блока
	/// </summary>
	/// <param name="id">Идентификатор блока</param>
	[HttpDelete(Delete)]
	public abstract Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int id);

	#endregion Методы
}
