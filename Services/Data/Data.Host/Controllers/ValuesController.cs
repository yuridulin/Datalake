using Datalake.Data.Host.Abstractions;
using Datalake.Data.Host.Models.Values;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Data.Host.Controllers;

/// <summary>
/// Взаимодействие с данными тегов
/// </summary>
[ApiController]
[Route("api/v1/values")]
public class ValuesController(
	IAuthenticatorService authenticator,
	IGetValuesService getValuesService,
	IManualWriteValuesService manualWriteValuesService) : ControllerBase
{
	/// <summary>
	/// <see cref="HttpMethod.Post" />: Получение значений на основании списка запросов
	/// </summary>
	/// <param name="requests">Список запросов с настройками</param>
	/// <returns>Список ответов на запросы</returns>
	[HttpPost]
	public async Task<ActionResult<List<ValuesResponse>>> GetAsync(
		[BindRequired, FromBody] ValuesRequest[] requests)
	{
		var user = authenticator.Authenticate(HttpContext);

		var responses = await getValuesService.GetAsync(user, requests);

		return responses;
	}

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Запись значений на основании списка запросов
	/// </summary>
	/// <param name="requests">Список запросов на изменение</param>
	/// <returns>Список измененных начений</returns>
	[HttpPut]
	public async Task<ActionResult<List<ValuesTagResponse>>> WriteAsync(
		[BindRequired, FromBody] ValueWriteRequest[] requests)
	{
		var user = authenticator.Authenticate(HttpContext);

		var responses = await manualWriteValuesService.WriteAsync(user, requests);

		return responses;
	}
}