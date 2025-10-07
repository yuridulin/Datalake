using Datalake.Data.Api.Models.Values;
using Datalake.Data.Application.Features.Values.Commands.ManualWriteValues;
using Datalake.Data.Application.Features.Values.Queries.GetValues;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Data.Host.Controllers;

/// <summary>
/// Данные тегов
/// </summary>
[ApiController]
[Route("api/v1/values")]
public class ValuesController(IAuthenticator authenticator) : ControllerBase
{
	/// <summary>
	/// <see cref="HttpMethod.Post" />: Получение значений на основании списка запросов
	/// </summary>
	/// <param name="requests">Список запросов с настройками</param>
	/// <returns>Список ответов на запросы</returns>
	[HttpPost]
	public async Task<ActionResult<IEnumerable<ValuesResponse>>> GetAsync(
		[FromServices] IGetValuesHandler handler,
		[BindRequired, FromBody] IEnumerable<ValuesRequest> requests,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		var data = await handler.HandleAsync(new()
		{
			User = user,
			Requests = requests,
		}, ct);

		return Ok(data);
	}

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Запись значений на основании списка запросов
	/// </summary>
	/// <param name="requests">Список запросов на изменение</param>
	/// <returns>Список измененных начений</returns>
	[HttpPut]
	public async Task<ActionResult<IEnumerable<ValuesTagResponse>>> WriteAsync(
		[FromServices] IManualWriteValuesHandler handler,
		[BindRequired, FromBody] IEnumerable<ValueWriteRequest> requests,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		var result = await handler.HandleAsync(new()
		{
			User = user,
			Requests = requests,
		}, ct);

		return Ok(result);
	}
}