using Datalake.PublicApi.Models.Values;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.PublicApi.Controllers;

/// <summary>
/// Взаимодействие с данными тегов
/// </summary>
[ApiController]
[Route("api/" + ControllerRoute)]
public abstract class ValuesControllerBase : ControllerBase
{
	/// <summary>
	/// Путь для получения текущих данные
	/// </summary>
	public const string LiveUrl = "api/" + ControllerRoute;

	/// <summary>
	/// Основной путь к контроллеру
	/// </summary>
	public const string ControllerRoute = "values";

	/// <summary>
	/// Получение значений на основании списка запросов
	/// </summary>
	/// <param name="requests">Список запросов с настройками</param>
	/// <returns>Список ответов на запросы</returns>
	[HttpPost]
	public abstract Task<List<ValuesResponse>> GetAsync(
		[BindRequired, FromBody] ValuesRequest[] requests);

	/// <summary>
	/// Запись значений на основании списка запросов
	/// </summary>
	/// <param name="requests">Список запросов на изменение</param>
	/// <returns>Список измененных начений</returns>
	[HttpPut]
	public abstract Task<List<ValuesTagResponse>> WriteAsync(
		[BindRequired, FromBody] ValueWriteRequest[] requests);
}