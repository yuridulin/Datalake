using Datalake.Database;
using Datalake.Database.InMemory.Repositories;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Blocks;
using Datalake.Server.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Server.Controllers;

/// <summary>
/// Взаимодействие с блоками
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class BlocksController(
	DatalakeContext db,
	BlocksMemoryRepository blocksRepository) : ApiControllerBase
{
	/// <summary>
	/// Создание нового блока на основании переданной информации
	/// </summary>
	/// <param name="blockInfo">Данные о новом блоке</param>
	/// <returns>Идентификатор блока</returns>
	[HttpPost]
	public async Task<ActionResult<BlockWithTagsInfo>> CreateAsync(
		[BindRequired, FromBody] BlockFullInfo blockInfo)
	{
		var user = Authenticate();

		return await blocksRepository.CreateAsync(db, user, blockInfo: blockInfo);
	}

	/// <summary>
	/// Создание нового отдельного блока с информацией по умолчанию
	/// </summary>
	/// <param name="parentId">Идентификатор родительского блока</param>
	/// <returns>Идентификатор блока</returns>
	[HttpPost("empty")]
	public async Task<ActionResult<BlockWithTagsInfo>> CreateEmptyAsync(
		[FromQuery] int? parentId)
	{
		var user = Authenticate();

		return await blocksRepository.CreateAsync(db, user, parentId: parentId);
	}

	/// <summary>
	/// Получение списка блоков с базовой информацией о них
	/// </summary>
	/// <returns>Список блоков</returns>
	[HttpGet]
	public ActionResult<BlockWithTagsInfo[]> Read()
	{
		var user = Authenticate();

		return blocksRepository.ReadAll(user);
	}

	/// <summary>
	/// Получение информации о выбранном блоке
	/// </summary>
	/// <param name="id">Идентификатор блока</param>
	/// <returns>Информация о блоке</returns>
	/// <exception cref="NotFoundException">Блок не найден по идентификатору</exception>
	[HttpGet("{id:int}")]
	public ActionResult<BlockFullInfo> Read(
		[BindRequired, FromRoute] int id)
	{
		var user = Authenticate();

		return blocksRepository.Read(user, id);
	}

	/// <summary>
	/// Получение иерархической структуры всех блоков
	/// </summary>
	/// <returns>Список обособленных блоков с вложенными блоками</returns>
	[HttpGet("tree")]
	public ActionResult<BlockTreeInfo[]> ReadAsTree()
	{
		var user = Authenticate();

		return blocksRepository.ReadAllAsTree(user);
	}

	/// <summary>
	/// Изменение блока
	/// </summary>
	/// <param name="id">Идентификатор блока</param>
	/// <param name="block">Новые данные блока</param>
	[HttpPut("{id:int}")]
	public async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int id,
		[BindRequired, FromBody] BlockUpdateRequest block)
	{
		var user = Authenticate();

		await blocksRepository.UpdateAsync(db, user, id, block);

		return NoContent();
	}

	/// <summary>
	/// Перемещение блока
	/// </summary>
	/// <param name="id">Идентификатор блока</param>
	/// <param name="parentId">Идентификатор нового родительского блока</param>
	[HttpPost("{id:int}/move")]
	public async Task<ActionResult> MoveAsync(
		[BindRequired, FromRoute] int id,
		[FromQuery] int? parentId)
	{
		var user = Authenticate();

		await blocksRepository.MoveAsync(db, user, id, parentId);

		return NoContent();
	}

	/// <summary>
	/// Удаление блока
	/// </summary>
	/// <param name="id">Идентификатор блока</param>
	[HttpDelete("{id:int}")]
	public async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int id)
	{
		var user = Authenticate();

		await blocksRepository.DeleteAsync(db, user, id);

		return NoContent();
	}
}
