using Datalake.Contracts.Models.Blocks;
using Datalake.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Shared.Hosting.AbstractControllers.Inventory;

/// <summary>
/// Взаимодействие с блоками
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "Inventory")]
[Route("api/v1/inventory/blocks")]
public abstract class InventoryBlocksControllerBase : ControllerBase
{
	/// <summary>
	/// Создание нового блока на основании переданной информации
	/// </summary>
	/// <param name="parentId">Идентификатор родительского блока</param>
	/// <param name="request">Данные о новом блоке</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Идентификатор блока</returns>
	[HttpPost]
	public abstract Task<ActionResult<int>> CreateAsync(
		[FromQuery] int? parentId,
		[FromBody, BindRequired] BlockCreateRequest request,
		CancellationToken ct = default);

	/// <summary>
	/// Получение списка блоков с базовой информацией о них
	/// </summary>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список блоков</returns>
	[HttpGet]
	public abstract Task<ActionResult<BlockWithTagsInfo[]>> GetAllAsync(
		CancellationToken ct = default);

	/// <summary>
	/// Получение информации о выбранном блоке
	/// </summary>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Информация о блоке</returns>
	[HttpGet("{blockId}")]
	public abstract Task<ActionResult<BlockDetailedInfo>> GetAsync(
		[BindRequired, FromRoute] int blockId,
		CancellationToken ct = default);

	/// <summary>
	/// Получение иерархической структуры всех блоков
	/// </summary>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список обособленных блоков с вложенными блоками</returns>
	[HttpGet("tree")]
	public abstract Task<ActionResult<BlockTreeInfo[]>> GetTreeAsync(
		CancellationToken ct = default);

	/// <summary>
	/// Изменение блока
	/// </summary>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="request">Новые данные блока</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("{blockId}")]
	public abstract Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int blockId,
		[BindRequired, FromBody] BlockUpdateRequest request,
		CancellationToken ct = default);

	/// <summary>
	/// Перемещение блока
	/// </summary>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="parentId">Идентификатор родительского блока</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("{blockId}/move")]
	public abstract Task<ActionResult> MoveAsync(
		[BindRequired, FromRoute] int blockId,
		[FromQuery] int? parentId,
		CancellationToken ct = default);

	/// <summary>
	/// Удаление блока
	/// </summary>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="ct">Токен отмены</param>
	[HttpDelete("{blockId}")]
	public abstract Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int blockId,
		CancellationToken ct = default);
}
