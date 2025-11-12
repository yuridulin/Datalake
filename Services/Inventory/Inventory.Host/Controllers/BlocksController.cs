using Datalake.Inventory.Api.Models.Blocks;
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
public class BlocksController(IAuthenticator authenticator) : ControllerBase
{
	/// <summary>
	/// <see cref="HttpMethod.Post" />: Создание нового блока на основании переданной информации
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="parentId">Идентификатор родительского блока</param>
	/// <param name="blockInfo">Данные о новом блоке</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Идентификатор блока</returns>
	[HttpPost]
	public async Task<ActionResult<int>> CreateAsync(
		[FromServices] ICreateBlockHandler handler,
		[FromQuery] int? parentId,
		[FromBody] BlockFullInfo? blockInfo,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var key = await handler.HandleAsync(new(user, blockInfo?.ParentId ?? parentId, blockInfo?.Name, blockInfo?.Description), ct);
		return key;
	}

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение списка блоков с базовой информацией о них
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список блоков</returns>
	[HttpGet]
	public async Task<ActionResult<BlockWithTagsInfo[]>> GetAllAsync(
		[FromServices] IGetBlocksWithTagsHandler handler,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		var data = await handler.HandleAsync(new(user), ct);

		return Ok(data);
	}

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение информации о выбранном блоке
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Информация о блоке</returns>
	[HttpGet("{blockId}")]
	public async Task<ActionResult<BlockFullInfo>> GetAsync(
		[FromServices] IGetBlockFullHandler handler,
		[BindRequired, FromRoute] int blockId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		var data = await handler.HandleAsync(new(user, blockId), ct);

		return Ok(data);
	}

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение иерархической структуры всех блоков
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список обособленных блоков с вложенными блоками</returns>
	[HttpGet("tree")]
	public async Task<ActionResult<BlockTreeInfo[]>> GetTreeAsync(
		[FromServices] IGetBlocksTreeHandler handler,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		var data = await handler.HandleAsync(new(user), ct);

		return Ok(data);
	}

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Изменение блока
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="request">Новые данные блока</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("{blockId}")]
	public async Task<ActionResult> UpdateAsync(
		[FromServices] IUpdateBlockHandler handler,
		[BindRequired, FromRoute] int blockId,
		[BindRequired, FromBody] BlockUpdateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		await handler.HandleAsync(new(
			user,
			blockId,
			request.Name,
			request.Description,
			request.Tags.Select(x => new BlockTagDto(x.Id, x.Name, x.Relation))), ct);

		return NoContent();
	}

	/// <summary>
	/// <see cref="HttpMethod.Post" />: Перемещение блока
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="parentId">Идентификатор родительского блока</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("{blockId}/move")]
	public async Task<ActionResult> MoveAsync(
		[FromServices] IMoveBlockHandler handler,
		[BindRequired, FromRoute] int blockId,
		[FromQuery] int? parentId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		await handler.HandleAsync(new(user, blockId, parentId), ct);

		return NoContent();
	}

	/// <summary>
	/// <see cref="HttpMethod.Delete" />: Удаление блока
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="ct">Токен отмены</param>
	[HttpDelete("{blockId}")]
	public async Task<ActionResult> DeleteAsync(
		[FromServices] IDeleteBlockHandler handler,
		[BindRequired, FromRoute] int blockId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		await handler.HandleAsync(new(user, blockId), ct);

		return NoContent();
	}
}