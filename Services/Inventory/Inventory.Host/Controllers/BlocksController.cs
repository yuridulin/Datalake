using Datalake.Contracts.Public.Models.Blocks;
using Datalake.Inventory.Application.Features.Blocks.Commands.CreateBlock;
using Datalake.Inventory.Application.Features.Blocks.Commands.DeleteBlock;
using Datalake.Inventory.Application.Features.Blocks.Commands.MoveBlock;
using Datalake.Inventory.Application.Features.Blocks.Commands.UpdateBlock;
using Datalake.Inventory.Application.Features.Blocks.Models;
using Datalake.Inventory.Application.Features.Blocks.Queries.GetBlockFull;
using Datalake.Inventory.Application.Features.Blocks.Queries.GetBlocksTree;
using Datalake.Inventory.Application.Features.Blocks.Queries.GetBlocksWithTags;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Inventory.Host.Controllers;

/// <summary>
/// Взаимодействие с блоками
/// </summary>
[ApiController]
[Route("api/blocks")]
public class BlocksController(
	IServiceProvider serviceProvider,
	IAuthenticator authenticator) : ControllerBase
{
	/// <summary>
	/// Создание нового блока на основании переданной информации
	/// </summary>
	/// <param name="parentId">Идентификатор родительского блока</param>
	/// <param name="blockInfo">Данные о новом блоке</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Идентификатор блока</returns>
	[HttpPost]
	public async Task<ActionResult<int>> CreateAsync(
		[FromQuery] int? parentId,
		[FromBody] BlockFullInfo? blockInfo,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<ICreateBlockHandler>();
		var key = await handler.HandleAsync(new(user, blockInfo?.ParentId ?? parentId, blockInfo?.Name, blockInfo?.Description), ct);
		return key;
	}

	/// <summary>
	/// Получение списка блоков с базовой информацией о них
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список блоков</returns>
	[HttpGet]
	public async Task<ActionResult<BlockWithTagsInfo[]>> GetAllAsync(
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetBlocksWithTagsHandler>();
		var data = await handler.HandleAsync(new(user), ct);

		return Ok(data);
	}

	/// <summary>
	/// Получение информации о выбранном блоке
	/// </summary>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Информация о блоке</returns>
	[HttpGet("{blockId}")]
	public async Task<ActionResult<BlockFullInfo>> GetAsync(
		[BindRequired, FromRoute] int blockId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetBlockFullHandler>();
		var data = await handler.HandleAsync(new(user, blockId), ct);

		return Ok(data);
	}

	/// <summary>
	/// Получение иерархической структуры всех блоков
	/// </summary>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список обособленных блоков с вложенными блоками</returns>
	[HttpGet("tree")]
	public async Task<ActionResult<BlockTreeInfo[]>> GetTreeAsync(
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetBlocksTreeHandler>();
		var data = await handler.HandleAsync(new(user), ct);

		return Ok(data);
	}

	/// <summary>
	/// Изменение блока
	/// </summary>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="request">Новые данные блока</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("{blockId}")]
	public async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int blockId,
		[BindRequired, FromBody] BlockUpdateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IUpdateBlockHandler>();
		await handler.HandleAsync(new(
			user,
			blockId,
			request.Name,
			request.Description,
			request.Tags.Select(x => new BlockTagDto(x.Id, x.Name, x.Relation))), ct);

		return NoContent();
	}

	/// <summary>
	/// Перемещение блока
	/// </summary>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="parentId">Идентификатор родительского блока</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("{blockId}/move")]
	public async Task<ActionResult> MoveAsync(
		[BindRequired, FromRoute] int blockId,
		[FromQuery] int? parentId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IMoveBlockHandler>();
		await handler.HandleAsync(new(user, blockId, parentId), ct);

		return NoContent();
	}

	/// <summary>
	/// Удаление блока
	/// </summary>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="ct">Токен отмены</param>
	[HttpDelete("{blockId}")]
	public async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int blockId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IDeleteBlockHandler>();
		await handler.HandleAsync(new(user, blockId), ct);

		return NoContent();
	}
}