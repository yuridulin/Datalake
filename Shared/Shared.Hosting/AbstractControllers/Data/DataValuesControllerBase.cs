using Datalake.Contracts.Models.Data.Values;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Shared.Hosting.AbstractControllers.Data;

/// <summary>
/// Данные тегов
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "Data")]
[Route("api/v1/data/values")]
public abstract class DataValuesControllerBase : ControllerBase
{
	/// <summary>
	/// Получение значений на основании списка запросов
	/// </summary>
	/// <param name="requests">Список запросов с настройками</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список ответов на запросы</returns>
	[HttpPost]
	public abstract Task<ActionResult<IEnumerable<ValuesResponse>>> GetAsync(
		[BindRequired, FromBody] IEnumerable<ValuesRequest> requests,
		CancellationToken ct = default);

	/// <summary>
	/// Запись значений на основании списка запросов
	/// </summary>
	/// <param name="requests">Список запросов на изменение</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список измененных значений</returns>
	[HttpPut]
	public abstract Task<ActionResult<IEnumerable<ValuesTagResponse>>> WriteAsync(
		[BindRequired, FromBody] IEnumerable<ValueWriteRequest> requests,
		CancellationToken ct = default);
}
