using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Models.Sources;
using Datalake.PublicApi.Models.States;
using Datalake.PublicApi.Models.Values;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.PublicApi.Controllers;

/// <summary>
/// Представление системной информации о работе сервера
/// </summary>
[ApiController]
[Route($"{Defaults.ApiRoot}/{ControllerRoute}")]
public abstract class StatesControllerBase : ControllerBase
{
	#region Константы путей

	/// <summary>
	/// Основной путь к контроллеру
	/// </summary>
	public const string ControllerRoute = "states";

	/// <inheritdoc cref="GetUsersAsync" />
	public const string Users = "users";

	/// <inheritdoc cref="GetSourcesAsync" />
	public const string Sources = "sources";

	/// <inheritdoc cref="GetTagsAsync" />
	public const string Tags = "tags";

	/// <inheritdoc cref="GetTagAsync" />
	public const string Tag = "tags/{id}";

	/// <inheritdoc cref="GetValuesAsync" />
	public const string Values = "values";

	/// <inheritdoc cref="GetTagsReceiveAsync" />
	public const string TagsReceive = "calculation";

	#endregion Константы путей

	#region Методы

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Информация о визитах пользователей
	/// </summary>
	/// <returns>Даты визитов, сопоставленные с идентификаторами пользователей</returns>
	[HttpGet(Users)]
	public abstract Task<ActionResult<Dictionary<Guid, DateTime>>> GetUsersAsync();

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Информация о подключении к источникам данных
	/// </summary>
	/// <returns></returns>
	[HttpGet(Sources)]
	public abstract Task<ActionResult<Dictionary<int, SourceStateInfo>>> GetSourcesAsync();

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Информация о подключении к источникам данных
	/// </summary>
	/// <returns></returns>
	[HttpGet(Tags)]
	public abstract Task<ActionResult<Dictionary<int, Dictionary<string, DateTime>>>> GetTagsAsync();

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Информация о подключении к источникам данных
	/// </summary>
	/// <returns></returns>
	[HttpGet(Tag)]
	public abstract Task<ActionResult<Dictionary<string, DateTime>>> GetTagAsync(
		[BindRequired, FromRoute] int id);

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение метрик запросов на чтение
	/// </summary>
	[HttpGet(Values)]
	public abstract Task<ActionResult<KeyValuePair<ValuesRequestKey, ValuesRequestUsageInfo>[]>> GetValuesAsync();

	/// <summary>
	/// <see cref="HttpMethod.Post"/>: Получение ошибок получения значений тегов
	/// </summary>
	/// <param name="identifiers">Идентификаторы тегов</param>
	[HttpPost(TagsReceive)]
	public abstract Task<ActionResult<Dictionary<int, TagReceiveState?>>> GetTagsReceiveAsync(
		[FromBody] int[]? identifiers);

	#endregion Методы
}
