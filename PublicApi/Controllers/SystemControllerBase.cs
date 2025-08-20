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
[Route("api/" + ControllerRoute)]
[ApiController]
public abstract class SystemControllerBase : ControllerBase
{
	/// <summary>
	/// Основной путь к контроллеру
	/// </summary>
	public const string ControllerRoute = "system";

	/// <summary>
	/// Получение даты последнего изменения структуры базы данных
	/// </summary>
	/// <returns>Дата в строковом виде</returns>
	[HttpGet("last")]
	public abstract ActionResult<string> GetLastUpdate();

	/// <summary>
	/// Получение списка сообщений
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
	[HttpGet("logs")]
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
	/// Информация о визитах пользователей
	/// </summary>
	/// <returns>Даты визитов, сопоставленные с идентификаторами пользователей</returns>
	[HttpGet("visits")]
	public abstract ActionResult<Dictionary<Guid, DateTime>> GetVisits();

	/// <summary>
	/// Информация о подключении к источникам данных
	/// </summary>
	/// <returns></returns>
	[HttpGet("sources")]
	public abstract ActionResult<Dictionary<int, SourceStateInfo>> GetSourcesStates();

	/// <summary>
	/// Информация о подключении к источникам данных
	/// </summary>
	/// <returns></returns>
	[HttpGet("tags")]
	public abstract ActionResult<Dictionary<int, Dictionary<string, DateTime>>> GetTagsStates();

	/// <summary>
	/// Информация о подключении к источникам данных
	/// </summary>
	/// <returns></returns>
	[HttpGet("tags/{id}")]
	public abstract ActionResult<Dictionary<string, DateTime>> GetTagState(
			[BindRequired, FromRoute] int id);

	/// <summary>
	/// Получение информации о настройках сервера
	/// </summary>
	/// <returns>Информация о настройках</returns>
	[HttpGet("settings")]
	public abstract ActionResult<SettingsInfo> GetSettings();

	/// <summary>
	/// Изменение информации о настройках сервера
	/// </summary>
	/// <param name="newSettings">Новые настройки сервера</param>
	[HttpPut("settings")]
	public abstract Task<ActionResult> UpdateSettingsAsync(
		[BindRequired, FromBody] SettingsInfo newSettings);

	/// <summary>
	/// Перестроение кэша
	/// </summary>
	/// <returns></returns>
	[HttpPut("restart/state")]
	public abstract Task<ActionResult> RestartStateAsync();

	/// <summary>
	/// Перестроение кэша текущих (последних) значений
	/// </summary>
	/// <returns></returns>
	[HttpPut("restart/values")]
	public abstract Task<ActionResult> RestartValuesAsync();

	/// <summary>
	/// Получение списка вычисленных прав доступа для каждого пользователя
	/// </summary>
	[HttpGet("access")]
	public abstract ActionResult<Dictionary<Guid, UserAuthInfo>> GetAccess();

	/// <summary>
	/// Получение метрик запросов на чтение
	/// </summary>
	[HttpGet("reads")]
	public abstract ActionResult<KeyValuePair<ValuesRequestKey, ValuesRequestUsageInfo>[]> GetReadMetricsAsync();
}