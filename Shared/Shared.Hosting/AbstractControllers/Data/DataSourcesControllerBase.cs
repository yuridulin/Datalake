using Datalake.Contracts.Models.Sources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Shared.Hosting.AbstractControllers.Data;

/// <summary>
/// Метрики источников данных
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "Data")]
[Route("api/v1/data/sources")]
public abstract class DataSourcesControllerBase : ControllerBase
{
	/// <summary>
	/// Получение данных о статистике сбора данных по источнику
	/// </summary>
	/// <param name="sourcesId">Список идентификаторов источников данных</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPost("activity")]
	public abstract Task<ActionResult<SourceActivityInfo[]>> GetActivityAsync(
		[BindRequired, FromBody] int[] sourcesId,
		CancellationToken ct = default);

	/// <summary>
	/// Получение удаленных значений с источника
	/// </summary>
	/// <param name="sourceId">Идентификатор источника данных</param>
	/// <param name="ct">Токен отмены</param>
	[HttpGet("{sourceId}/items")]
	public abstract Task<ActionResult<List<SourceItemInfo>>> GetItemsAsync(
		[BindRequired, FromRoute] int sourceId,
		CancellationToken ct = default);
}
