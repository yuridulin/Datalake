using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Blocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.PublicApi.Controllers;

/// <summary>
/// Взаимодействие с блоками
/// </summary>
[Route("api/" + ControllerRoute)]
[ApiController]
public abstract class BlocksControllerBase : ControllerBase
{
	/// <summary>
	/// Основной путь к контроллеру
	/// </summary>
	public const string ControllerRoute = "blocks";

	/// <summary>
	/// Создание нового блока на основании переданной информации
	/// </summary>
	/// <param name="blockInfo">Данные о новом блоке</param>
	/// <returns>Идентификатор блока</returns>
	[HttpPost]
	public abstract Task<ActionResult<BlockWithTagsInfo>> CreateAsync(
		[BindRequired, FromBody] BlockFullInfo blockInfo);

	/// <summary>
	/// Создание нового отдельного блока с информацией по умолчанию
	/// </summary>
	/// <param name="parentId">Идентификатор родительского блока</param>
	/// <returns>Идентификатор блока</returns>
	[HttpPost("empty")]
	public abstract Task<ActionResult<BlockWithTagsInfo>> CreateEmptyAsync(
		[FromQuery] int? parentId);

	/// <summary>
	/// Получение списка блоков с базовой информацией о них
	/// </summary>
	/// <returns>Список блоков</returns>
	[HttpGet]
	public abstract ActionResult<BlockWithTagsInfo[]> GetAll();

	/// <summary>
	/// Получение информации о выбранном блоке
	/// </summary>
	/// <param name="id">Идентификатор блока</param>
	/// <returns>Информация о блоке</returns>
	/// <exception cref="NotFoundException">Блок не найден по идентификатору</exception>
	[HttpGet("{id:int}")]
	public abstract ActionResult<BlockFullInfo> Get(
		[BindRequired, FromRoute] int id);

	/// <summary>
	/// Получение иерархической структуры всех блоков
	/// </summary>
	/// <returns>Список обособленных блоков с вложенными блоками</returns>
	[HttpGet("tree")]
	public abstract ActionResult<BlockTreeInfo[]> GetTree();

	/// <summary>
	/// Изменение блока
	/// </summary>
	/// <param name="id">Идентификатор блока</param>
	/// <param name="block">Новые данные блока</param>
	[HttpPut("{id:int}")]
	public abstract Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int id,
		[BindRequired, FromBody] BlockUpdateRequest block);

	/// <summary>
	/// Перемещение блока
	/// </summary>
	/// <param name="id">Идентификатор блока</param>
	/// <param name="parentId">Идентификатор нового родительского блока</param>
	[HttpPost("{id:int}/move")]
	public abstract Task<ActionResult> MoveAsync(
		[BindRequired, FromRoute] int id,
		[FromQuery] int? parentId);

	/// <summary>
	/// Удаление блока
	/// </summary>
	/// <param name="id">Идентификатор блока</param>
	[HttpDelete("{id:int}")]
	public abstract Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int id);
}