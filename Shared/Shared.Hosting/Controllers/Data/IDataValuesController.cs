using Datalake.Contracts.Public.Models.Data.Values;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Shared.Hosting.Controllers.Data;

public class TestCase
{
	public required string Message { get; set; }
}

/// <summary>
/// Данные тегов
/// </summary>
[ApiController]
[Route("api/v1/data/values")]
public abstract class BaseValuesController : ControllerBase
{
	[HttpGet]
	public abstract Task<ActionResult<TestCase>> GetAsync(CancellationToken ct = default);

	/// <summary>
	/// Получение значений на основании списка запросов
	/// </summary>
	/// <param name="requests">Список запросов с настройками</param>
	/// <returns>Список ответов на запросы</returns>
	[HttpPost]
	public abstract Task<ActionResult<IEnumerable<ValuesResponse>>> GetAsync(
		[BindRequired, FromBody] IEnumerable<ValuesRequest> requests,
		CancellationToken ct = default);

	/// <summary>
	/// Запись значений на основании списка запросов
	/// </summary>
	/// <param name="requests">Список запросов на изменение</param>
	/// <returns>Список измененных значений</returns>
	[HttpPut]
	public abstract Task<ActionResult<IEnumerable<ValuesTagResponse>>> WriteAsync(
		[BindRequired, FromBody] IEnumerable<ValueWriteRequest> requests,
		CancellationToken ct = default);
}
