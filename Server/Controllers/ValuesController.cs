using Datalake.Database;
using Datalake.Database.Repositories;
using Datalake.PublicApi.Models.Values;
using Datalake.Server.Controllers.Base;
using Datalake.Server.Services.StateManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Server.Controllers;

/// <summary>
/// Взаимодействие с данными тегов
/// </summary>
[ApiController]
[Route("api/Tags/[controller]")]
public class ValuesController(
	DatalakeContext db,
	TagsStateService tagsStateService) : ApiControllerBase
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
		var user = Authenticate();

		tagsStateService.UpdateTagState(requests);
		var responses = await ValuesRepository.GetValuesAsync(db, user, requests);

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
		var user = Authenticate();

		// Флаг отключает проверку на новизну значения по сравнению с текущим
		var responses = await ValuesRepository.WriteValuesAsync(db, user, requests, overrided: true);

		return responses;
	}
}
