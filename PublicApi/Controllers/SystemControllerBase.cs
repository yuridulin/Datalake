using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.LogModels;
using Datalake.PublicApi.Models.Settings;
using Datalake.PublicApi.Models.Sources;
using Datalake.PublicApi.Models.Values;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.PublicApi.Controllers;

/// <summary>
/// Представление системной информации о работе сервера
/// </summary>
[ApiController]
[Route($"{Defaults.ApiRoot}/{ControllerRoute}")]
public abstract class SystemControllerBase : ControllerBase
{
	#region Константы путей

	/// <summary>
	/// Основной путь к контроллеру
	/// </summary>
	public const string ControllerRoute = "system";

	/// <inheritdoc cref="GetLastUpdateAsync" />
	public const string Last = "last";

	/// <inheritdoc cref="GetLogsAsync" />
	public const string Logs = "logs";

	/// <inheritdoc cref="GetVisitsAsync" />
	public const string Visits = "visits";

	/// <inheritdoc cref="GetSourcesStatesAsync" />
	public const string Sources = "sources";

	/// <inheritdoc cref="GetTagsStatesAsync" />
	public const string Tags = "tags";

	/// <inheritdoc cref="GetTagStateAsync" />
	public const string TagState = "tags/{id}";

	/// <inheritdoc cref="GetSettingsAsync" />
	public const string Settings = "settings";

	/// <inheritdoc cref="UpdateSettingsAsync" />
	public const string UpdateSettings = "settings";

	/// <inheritdoc cref="RestartStateAsync" />
	public const string RestartState = "restart/state";

	/// <inheritdoc cref="RestartValuesAsync" />
	public const string RestartValues = "restart/values";

	/// <inheritdoc cref="GetAccessAsync" />
	public const string Access = "access";

	/// <inheritdoc cref="GetReadMetricsAsync" />
	public const string ReadMetrics = "reads";

	#endregion Константы путей

	#region Методы

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение даты последнего изменения структуры базы данных
	/// </summary>
	/// <returns>Дата в строковом виде</returns>
	[HttpGet(Last)]
	public abstract Task<ActionResult<string>> GetLastUpdateAsync();

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение списка сообщений
	/// </summary>
	/// <param name="lastId">Идентификатор сообщения, с которого начать отсчёт количества в сторону более поздних</param>
	/// <param name="firstId">Идентификатор сообщения, с которого начать отсчёт количества в сторону более ранних</param>
	/// <param name="take">Сколько сообщений получить за этот запрос</param>
	/// <param name="source">Идентификатор затронутого источника</param>
	/// <param name="block">Идентификатор затронутого блока</param>
	/// <param name="tag">Идентификатор затронутого тега</param>
	/// <param name="user">Идентификатор затронутого пользователя</param>
	/// <param name="group">Идентификатор затронутой группы пользователей</param>
	/// <param name="categories">Выбранные категории сообщений</param>
	/// <param name="types">Выбранные типы сообщений</param>
	/// <param name="author">Идентификатор пользователя, создавшего сообщение</param>
	/// <returns>Список сообщений</returns>
	[HttpGet(Logs)]
	public abstract Task<ActionResult<LogInfo[]>> GetLogsAsync(
		[FromQuery] int? lastId = null,
		[FromQuery] int? firstId = null,
		[FromQuery] int? take = null,
		[FromQuery] int? source = null,
		[FromQuery] int? block = null,
		[FromQuery] Guid? tag = null,
		[FromQuery] Guid? user = null,
		[FromQuery] Guid? group = null,
		[FromQuery(Name = "categories[]")] LogCategory[]? categories = null,
		[FromQuery(Name = "types[]")] LogType[]? types = null,
		[FromQuery] Guid? author = null);

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Информация о визитах пользователей
	/// </summary>
	/// <returns>Даты визитов, сопоставленные с идентификаторами пользователей</returns>
	[HttpGet(Visits)]
	public abstract Task<ActionResult<Dictionary<Guid, DateTime>>> GetVisitsAsync();

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Информация о подключении к источникам данных
	/// </summary>
	/// <returns></returns>
	[HttpGet(Sources)]
	public abstract Task<ActionResult<Dictionary<int, SourceStateInfo>>> GetSourcesStatesAsync();

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Информация о подключении к источникам данных
	/// </summary>
	/// <returns></returns>
	[HttpGet(Tags)]
	public abstract Task<ActionResult<Dictionary<int, Dictionary<string, DateTime>>>> GetTagsStatesAsync();

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Информация о подключении к источникам данных
	/// </summary>
	/// <returns></returns>
	[HttpGet(TagState)]
	public abstract Task<ActionResult<Dictionary<string, DateTime>>> GetTagStateAsync(
			[BindRequired, FromRoute] int id);

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение информации о настройках сервера
	/// </summary>
	/// <returns>Информация о настройках</returns>
	[HttpGet(Settings)]
	public abstract Task<ActionResult<SettingsInfo>> GetSettingsAsync();

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Изменение информации о настройках сервера
	/// </summary>
	/// <param name="newSettings">Новые настройки сервера</param>
	[HttpPut(UpdateSettings)]
	public abstract Task<ActionResult> UpdateSettingsAsync(
		[BindRequired, FromBody] SettingsInfo newSettings);

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Перестроение кэша
	/// </summary>
	/// <returns></returns>
	[HttpPut(RestartState)]
	public abstract Task<ActionResult> RestartStateAsync();

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Перестроение кэша текущих (последних) значений
	/// </summary>
	/// <returns></returns>
	[HttpPut(RestartValues)]
	public abstract Task<ActionResult> RestartValuesAsync();

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение списка вычисленных прав доступа для каждого пользователя
	/// </summary>
	[HttpGet(Access)]
	public abstract Task<ActionResult<Dictionary<Guid, UserAuthInfo>>> GetAccessAsync();

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение метрик запросов на чтение
	/// </summary>
	[HttpGet(ReadMetrics)]
	public abstract Task<ActionResult<KeyValuePair<ValuesRequestKey, ValuesRequestUsageInfo>[]>> GetReadMetricsAsync();

	#endregion Методы
}
