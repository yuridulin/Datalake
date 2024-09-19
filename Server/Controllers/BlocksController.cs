using Datalake.ApiClasses.Exceptions;
using Datalake.ApiClasses.Models.Blocks;
using Datalake.Database.Repositories;
using Datalake.Server.Controllers.Base;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Server.Controllers;

/// <summary>
/// Взаимодействие с блоками
/// </summary>
/// <param name="blocksRepository">Репозиторий</param>
[Route("api/[controller]")]
[ApiController]
public class BlocksController(BlocksRepository blocksRepository) : ApiControllerBase
{
	/// <summary>
	/// Создание нового блока на основании переданной информации
	/// </summary>
	/// <param name="blockInfo">Данные о новом блоке</param>
	/// <returns>Идентификатор блока</returns>
	[HttpPost]
	public async Task<ActionResult<int>> CreateAsync(
		[BindRequired, FromBody] BlockInfo blockInfo)
	{
		var user = Authenticate();

		return await blocksRepository.CreateAsync(user, blockInfo);
	}

	/// <summary>
	/// Создание нового отдельного блока с информацией по умолчанию
	/// </summary>
	/// <returns>Идентификатор блока</returns>
	[HttpPost("empty")]
	public async Task<ActionResult<int>> CreateEmptyAsync()
	{
		var user = Authenticate();

		return await blocksRepository.CreateAsync(user);
	}

	/// <summary>
	/// Получение списка блоков с базовой информацией о них
	/// </summary>
	/// <returns>Список блоков</returns>
	[HttpGet]
	public async Task<ActionResult<BlockSimpleInfo[]>> ReadAsync()
	{
		return await blocksRepository.GetSimpleInfo()
			.ToArrayAsync();
	}

	/// <summary>
	/// Получение информации о выбранном блоке
	/// </summary>
	/// <param name="id">Идентификатор блока</param>
	/// <returns>Информация о блоке</returns>
	/// <exception cref="NotFoundException">Блок не найден по идентификатору</exception>
	[HttpGet("{id:int}")]
	public async Task<ActionResult<BlockInfo>> ReadAsync(
		[BindRequired, FromRoute] int id)
	{
		return await blocksRepository.GetInfoWithAllRelations()
			.Where(x => x.Id == id)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"Сущность #{id}");
	}

	/// <summary>
	/// Получение иерархической структуры всех блоков
	/// </summary>
	/// <returns>Список обособленных блоков с вложенными блоками</returns>
	[HttpGet("tree")]
	public async Task<ActionResult<BlockTreeInfo[]>> ReadAsTreeAsync()
	{
		return await blocksRepository.GetTreeAsync();
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

		await blocksRepository.UpdateAsync(user, id, block);

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

		await blocksRepository.DeleteAsync(user, id);

		return NoContent();
	}
}
