using Datalake.ApiClasses.Models.Values;
using Datalake.Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Server.Controllers;

/// <summary>
/// Взаимодействие с данными тегов
/// </summary>
/// <param name="valuesRepository">Репозиторий</param>
[ApiController]
[Route("api/Tags/[controller]")]
public class ValuesController(ValuesRepository valuesRepository) : ControllerBase
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
		var responses = await valuesRepository.GetValuesAsync(requests);

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
		var responses = await valuesRepository.WriteValuesAsync(requests);

		return responses;
	}
}
