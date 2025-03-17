using Datalake.Database;
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
public class BlocksController(DatalakeContext db) : ApiControllerBase
{
	/// <summary>
	/// Создание нового блока на основании переданной информации
	/// </summary>
	/// <param name="blockInfo">Данные о новом блоке</param>
	/// <returns>Идентификатор блока</returns>
	[HttpPost]
	public async Task<ActionResult<int>> CreateAsync(
		[BindRequired, FromBody] BlockFullInfo blockInfo)
	{
		var user = Authenticate();

		return await db.BlocksRepository.CreateAsync(user, blockInfo: blockInfo);
	}

	/// <summary>
	/// Создание нового отдельного блока с информацией по умолчанию
	/// </summary>
	/// <param name="parentId">Идентификатор родительского блока</param>
	/// <returns>Идентификатор блока</returns>
	[HttpPost("empty")]
	public async Task<ActionResult<int>> CreateEmptyAsync(
		[FromQuery] int? parentId)
	{
		var user = Authenticate();

		return await db.BlocksRepository.CreateAsync(user, parentId: parentId);
	}

	/// <summary>
	/// Получение списка блоков с базовой информацией о них
	/// </summary>
	/// <returns>Список блоков</returns>
	[HttpGet]
	public async Task<ActionResult<BlockWithTagsInfo[]>> ReadAsync()
	{
		var user = Authenticate();

		return await db.BlocksRepository.ReadAllAsync(user);
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

		return await db.BlocksRepository.ReadAsync(user, id);
	}

	/// <summary>
	/// Получение иерархической структуры всех блоков
	/// </summary>
	/// <returns>Список обособленных блоков с вложенными блоками</returns>
	[HttpGet("tree")]
	public async Task<ActionResult<BlockTreeInfo[]>> ReadAsTreeAsync()
	{
		var user = Authenticate();

		return await db.BlocksRepository.ReadAllAsTreeAsync(user);
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

		await db.BlocksRepository.UpdateAsync(user, id, block);

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

		await db.BlocksRepository.MoveAsync(user, id, parentId);

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

		await db.BlocksRepository.DeleteAsync(user, id);

		return NoContent();
	}
}
