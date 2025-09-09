using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Models.Values;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.PublicApi.Controllers;

/// <summary>
/// Взаимодействие с данными тегов
/// </summary>
[ApiController]
[Route($"{Defaults.ApiRoot}/{ControllerRoute}")]
public abstract class ValuesControllerBase : ControllerBase
{
	#region Константы путей

	/// <summary>
	/// Путь для получения текущих данные
	/// </summary>
	public static string LiveUrl => $"api/{ControllerRoute}/{Get}";

	/// <summary>
	/// Основной путь к контроллеру
	/// </summary>
	public const string ControllerRoute = "values";

	/// <inheritdoc cref="GetAsync(ValuesRequest[])" />
	public const string Get = "";

	/// <inheritdoc cref="WriteAsync(ValueWriteRequest[])" />
	public const string Write = "";

	#endregion Константы путей

	#region Методы

	/// <summary>
	/// <see cref="HttpMethod.Post" />: Получение значений на основании списка запросов
	/// </summary>
	/// <param name="requests">Список запросов с настройками</param>
	/// <returns>Список ответов на запросы</returns>
	[HttpPost(Get)]
	public abstract Task<ActionResult<List<ValuesResponse>>> GetAsync(
		[BindRequired, FromBody] ValuesRequest[] requests);

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Запись значений на основании списка запросов
	/// </summary>
	/// <param name="requests">Список запросов на изменение</param>
	/// <returns>Список измененных начений</returns>
	[HttpPut(Write)]
	public abstract Task<ActionResult<List<ValuesTagResponse>>> WriteAsync(
		[BindRequired, FromBody] ValueWriteRequest[] requests);

	#endregion Методы
}