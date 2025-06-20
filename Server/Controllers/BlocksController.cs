using Datalake.Database;
using Datalake.Database.InMemory;
using Datalake.Database.Repositories;
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
	BlocksMemoryRepository repository,
	DerivedDataStore dataStore) : ApiControllerBase
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

		return await BlocksRepository.CreateAsync(db, user, blockInfo: blockInfo);
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

		return await BlocksRepository.CreateAsync(db, user, parentId: parentId);
	}

	/// <summary>
	/// Получение списка блоков с базовой информацией о них
	/// </summary>
	/// <returns>Список блоков</returns>
	[HttpGet]
	public async Task<ActionResult<BlockWithTagsInfo[]>> ReadAsync()
	{
		var user = Authenticate();

		return await BlocksRepository.ReadAllAsync(db, user);
	}

	/// <summary>
	/// Получение информации о выбранном блоке
	/// </summary>
	/// <param name="id">Идентификатор блока</param>
	/// <returns>Информация о блоке</returns>
	/// <exception cref="NotFoundException">Блок не найден по идентификатору</exception>
	[HttpGet("{id:int}")]
	public async Task<ActionResult<BlockFullInfo>> ReadAsync(
		[BindRequired, FromRoute] int id)
	{
		var user = Authenticate();

		return await BlocksRepository.ReadAsync(db, user, id);
	}

	/// <summary>
	/// Получение иерархической структуры всех блоков
	/// </summary>
	/// <returns>Список обособленных блоков с вложенными блоками</returns>
	[HttpGet("tree")]
	public async Task<ActionResult<BlockTreeInfo[]>> ReadAsTreeAsync()
	{
		var user = Authenticate();

		return await BlocksRepository.ReadAllAsTreeAsync(db, user);
	}

	/// <summary>
	/// Получение иерархической структуры всех блоков
	/// </summary>
	/// <returns>Список обособленных блоков с вложенными блоками</returns>
	[HttpGet("tree2")]
	public ActionResult<BlockTreeInfo[]> ReadAsTree2()
	{
		Authenticate();

		return dataStore.BlocksTree();
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

		await BlocksRepository.UpdateAsync(db, user, id, block);

		return NoContent();
	}

	/// <summary>
	/// Изменение блока
	/// </summary>
	/// <param name="id">Идентификатор блока</param>
	/// <param name="block">Новые данные блока</param>
	[HttpPut("{id:int}/2")]
	public async Task<ActionResult> UpdateAsync2(
		[BindRequired, FromRoute] int id,
		[BindRequired, FromBody] BlockUpdateRequest block)
	{
		/*var user = */Authenticate();

		await repository.UpdateBlock(db, id, block);
		//await BlocksRepository.UpdateAsync(db, user, id, block);

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

		await BlocksRepository.MoveAsync(db, user, id, parentId);

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

		await BlocksRepository.DeleteAsync(db, user, id);

		return NoContent();
	}
}
