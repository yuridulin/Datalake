using Datalake.Contracts.Models.Sources;
using Datalake.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Shared.Hosting.AbstractControllers.Inventory;

/// <summary>
/// Источники данных
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "Inventory")]
[Route("api/v1/inventory/sources")]
public abstract class InventorySourcesControllerBase : ControllerBase
{
	/// <summary>
	/// Создание источника
	/// </summary>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Идентификатор источника</returns>
	[HttpPost]
	public abstract Task<ActionResult<int>> CreateAsync(
		CancellationToken ct = default);

	/// <summary>
	/// Получение данных о источнике
	/// </summary>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Данные о источнике</returns>
	[HttpGet("{sourceId}")]
	public abstract Task<ActionResult<SourceWithSettingsAndTagsInfo>> GetAsync(
		[BindRequired, FromRoute] int sourceId,
		CancellationToken ct = default);

	/// <summary>
	/// Получение списка источников
	/// </summary>
	/// <param name="withCustom">Включить ли в список системные источники</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список источников</returns>
	[HttpGet]
	public abstract Task<ActionResult<List<SourceWithSettingsInfo>>> GetAllAsync(
		[FromQuery] bool withCustom = false,
		CancellationToken ct = default);

	/// <summary>
	/// Изменение источника
	/// </summary>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <param name="request">Новые данные источника</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("{sourceId}")]
	public abstract Task<ActionResult<bool>> UpdateAsync(
		[BindRequired, FromRoute] int sourceId,
		[BindRequired, FromBody] SourceUpdateRequest request,
		CancellationToken ct = default);

	/// <summary>
	/// Удаление источника
	/// </summary>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <param name="ct">Токен отмены</param>
	[HttpDelete("{sourceId}")]
	public abstract Task<ActionResult<bool>> DeleteAsync(
		[BindRequired, FromRoute] int sourceId,
		CancellationToken ct = default);
}
