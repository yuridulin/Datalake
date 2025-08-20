using Datalake.Database;
using Datalake.Database.Repositories;
using Datalake.PublicApi.Models.Values;
using Datalake.Server.Services.Auth;
using Datalake.Server.Services.Maintenance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Diagnostics;

namespace Datalake.Server.Controllers;

/// <summary>
/// Взаимодействие с данными тегов
/// </summary>
[ApiController]
[Route("api/Tags/[controller]")]
public class ValuesController(
	DatalakeContext db,
	AuthenticationService authenticator,
	ValuesRepository valuesRepository,
	TagsStateService tagsStateService,
	RequestsStateService requestsStateService) : ControllerBase
{
	/// <summary>
	/// Путь для получения текущих данные
	/// </summary>
	public const string LiveUrl = "api/Tags/values";

	/// <summary>
	/// Получение значений на основании списка запросов
	/// </summary>
	/// <param name="requests">Список запросов с настройками</param>
	/// <returns>Список ответов на запросы</returns>
	[HttpPost]
	public async Task<List<ValuesResponse>> GetAsync(
		[BindRequired, FromBody] ValuesRequest[] requests)
	{
		var user = authenticator.Authenticate(HttpContext);

		var sw = Stopwatch.StartNew();
		var responses = await valuesRepository.GetValuesAsync(db, user, requests);
		sw.Stop();

		tagsStateService.UpdateTagState(requests);
		requestsStateService.RecordBatch(requests, sw.Elapsed, responses);

		return responses;
	}

	/// <summary>
	/// Запись значений на основании списка запросов
	/// </summary>
	/// <param name="requests">Список запросов на изменение</param>
	/// <returns>Список измененных начений</returns>
	[HttpPut]
	public async Task<List<ValuesTagResponse>> WriteAsync(
		[BindRequired, FromBody] ValueWriteRequest[] requests)
	{
		var user = authenticator.Authenticate(HttpContext);

		var responses = await valuesRepository.WriteManualValuesAsync(db, user, requests);

		return responses;
	}
}
