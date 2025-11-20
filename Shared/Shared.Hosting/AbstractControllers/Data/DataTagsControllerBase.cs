using Datalake.Contracts.Models.Tags;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Shared.Hosting.AbstractControllers.Data;

/// <summary>
/// Метрики тегов
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "Data")]
[Route("api/v1/data/tags")]
public abstract class DataTagsControllerBase : ControllerBase
{
	/// <summary>
	/// Получение данных о использовании тегов
	/// </summary>
	/// <param name="request">Список идентификаторов запрошенных тегов</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Объект статистики использования, сопоставленный с идентификаторами</returns>
	[HttpPost("usage")]
	public abstract Task<ActionResult<IDictionary<int, IDictionary<string, DateTime>>>> GetUsageAsync(
		[BindRequired, FromBody] TagMetricRequest request,
		CancellationToken ct = default);

	/// <summary>
	/// Запись данных о состоянии последнего получения/вычисления значений тегов
	/// </summary>
	/// <param name="request">Список идентификаторов запрошенных тегов</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Объект состояния последнего получениея/вычисления, сопоставленный с идентификаторами</returns>
	[HttpPost("status")]
	public abstract Task<ActionResult<IDictionary<int, string>>> GetStatusAsync(
		[BindRequired, FromBody] TagMetricRequest request,
		CancellationToken ct = default);
}
