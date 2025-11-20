using Datalake.Contracts.Models.Tags;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Shared.Hosting.AbstractControllers.Inventory;

/// <summary>
/// Теги
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "Inventory")]
[Route("api/v1/inventory/tags")]
public abstract class InventoryTagsControllerBase : ControllerBase
{
	/// <summary>
	/// Создание нового тега
	/// </summary>
	/// <param name="request">Необходимые данные для создания тега</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Идентификатор нового тега в локальной базе данных</returns>
	[HttpPost]
	public abstract Task<ActionResult<TagInfo>> CreateAsync(
		[BindRequired, FromBody] TagCreateRequest request,
		CancellationToken ct = default);

	/// <summary>
	/// Получение информации о конкретном теге, включая информацию о источнике и настройках получения данных
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Объект информации о теге</returns>
	[HttpGet("{tagId}")]
	public abstract Task<ActionResult<TagFullInfo>> GetAsync(
		[FromRoute] int tagId,
		CancellationToken ct = default);

	/// <summary>
	/// Получение списка тегов, включая информацию о источниках и настройках получения данных
	/// </summary>
	/// <param name="sourceId">Идентификатор источника. Если указан, будут выбраны теги только этого источника</param>
	/// <param name="tagsId">Список локальных идентификаторов тегов</param>
	/// <param name="tagsGuid">Список глобальных идентификаторов тегов</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Плоский список объектов информации о тегах</returns>
	[HttpGet]
	public abstract Task<ActionResult<TagInfo[]>> GetAllAsync(
		[FromQuery] int? sourceId,
		[FromQuery] int[]? tagsId,
		[FromQuery] Guid[]? tagsGuid,
		CancellationToken ct = default);

	/// <summary>
	/// Изменение тега
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="request">Новые данные тега</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("{tagId}")]
	public abstract Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int tagId,
		[BindRequired, FromBody] TagUpdateRequest request,
		CancellationToken ct = default);

	/// <summary>
	/// Удаление тега
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="ct">Токен отмены</param>
	[HttpDelete("{tagId}")]
	public abstract Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int tagId,
		CancellationToken ct = default);
}
