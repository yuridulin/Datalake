using Datalake.Database;
using Datalake.Database.Repositories;
using Datalake.Database.Services;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.LogModels;
using Datalake.PublicApi.Models.Metrics;
using Datalake.PublicApi.Models.Settings;
using Datalake.Server.BackgroundServices.SettingsHandler;
using Datalake.Server.Controllers.Base;
using Datalake.Server.Services.StateManager;
using Datalake.Server.Services.StateManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Server.Controllers;

/// <summary>
/// Представление системной информации о работе сервера
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class SystemController(
	DatalakeContext db,
	SourcesStateService sourcesStateService,
	TagsStateService tagsStateService,
	UsersStateService usersStateService,
	ISettingsUpdater settingsService) : ApiControllerBase
{
	/// <summary>
	/// Получение даты последнего изменения структуры базы данных
	/// </summary>
	/// <returns>Дата в строковом виде</returns>
	[HttpGet("last")]
	public ActionResult<string> GetLastUpdate()
	{
		var lastUpdate = SystemRepository.LastUpdate;
		return lastUpdate.ToString(DateFormats.HierarchicalWithMilliseconds);
	}

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
	public async Task<ActionResult<LogInfo[]>> GetLogsAsync(
		[FromQuery] int? lastId = null,
		[FromQuery] int? firstId = null,
		[FromQuery] int? take = null,
		[FromQuery] int? source = null,
		[FromQuery] int? block = null,
		[FromQuery] Guid? tag = null,
		[FromQuery] Guid? user = null,
		[FromQuery] Guid? group = null,
		[FromQuery(Name = nameof(categories) + "[]")] LogCategory[]? categories = null,
		[FromQuery(Name = nameof(types) + "[]")] LogType[]? types = null,
		[FromQuery] Guid? author = null)
	{
		var userAuth = Authenticate();

		return await SystemRepository.GetLogsAsync(db, userAuth, lastId, firstId, take, source, block, tag, user, group, categories, types, author);
	}

	/// <summary>
	/// Информация о визитах пользователей
	/// </summary>
	/// <returns>Даты визитов, сопоставленные с идентификаторами пользователей</returns>
	[HttpGet("visits")]
	public ActionResult<Dictionary<Guid, DateTime>> GetVisits()
	{
		var user = Authenticate();

		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Viewer);

		return usersStateService.State;
	}

	/// <summary>
	/// Информация о подключении к источникам данных
	/// </summary>
	/// <returns></returns>
	[HttpGet("sources")]
	public ActionResult<Dictionary<int, SourceState>> GetSourcesStates()
	{
		var user = Authenticate();

		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Viewer);

		return sourcesStateService.State
			.Where(x => AccessRepository.HasAccessToSource(user, AccessType.Viewer, x.Key))
			.ToDictionary();
	}

	/// <summary>
	/// Информация о подключении к источникам данных
	/// </summary>
	/// <returns></returns>
	[HttpGet("tags")]
	public ActionResult<Dictionary<Guid, Dictionary<string, DateTime>>> GetTagsStates()
	{
		var user = Authenticate();

		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Viewer);

		return tagsStateService.GetTagsStates()
			.Where(x => AccessRepository.HasAccessToTag(user, AccessType.Viewer, x.Key))
			.ToDictionary();
	}

	/// <summary>
	/// Получение информации о настройках сервера
	/// </summary>
	/// <returns>Информация о настройках</returns>
	[HttpGet("settings")]
	public async Task<ActionResult<SettingsInfo>> GetSettingsAsync()
	{
		var user = Authenticate();

		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Editor);

		var info = await SystemRepository.GetSettingsAsync(db, user);

		return info;
	}

	/// <summary>
	/// Изменение информации о настройках сервера
	/// </summary>
	/// <param name="newSettings">Новые настройки сервера</param>
	[HttpPut("settings")]
	public async Task<ActionResult> UpdateSettingsAsync(
		[BindRequired][FromBody] SettingsInfo newSettings)
	{
		var user = Authenticate();

		await SystemRepository.UpdateSettingsAsync(db, user, newSettings);
		await settingsService.WriteStartipFileAsync(db);

		return NoContent();
	}

	/// <summary>
	/// Перестроение кэша и перезапуск всех сборщиков
	/// </summary>
	/// <returns></returns>
	[HttpPut("restart/storage")]
	public async Task<ActionResult> RestartStorageAsync()
	{
		var user = Authenticate();

		await SystemRepository.RebuildStorageCacheAsync(db, user);

		return NoContent();
	}

	/// <summary>
	/// Перестроение кэша вычисленных прав доступа
	/// </summary>
	/// <returns></returns>
	[HttpPut("restart/access")]
	public async Task<ActionResult> RestartAccessAsync()
	{
		var user = Authenticate();
		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		await AccessRepository.RebuildUserRightsCacheAsync(db);

		return NoContent();
	}

	/// <summary>
	/// Получение списка вычисленных прав доступа для каждого пользователя
	/// </summary>
	/// <returns></returns>
	[HttpGet("access")]
	public ActionResult<Dictionary<Guid, UserAuthInfo>> GetAccess()
	{
		var user = Authenticate();
		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		return Ok(AccessRepository.UserRights.ToDictionary(x => x.Key, x => x.Value));
	}

	/// <summary>
	/// Получение списка сохраненных метрик
	/// </summary>
	/// <returns>Список метрик</returns>
	[HttpGet("metrics/read")]
	public ActionResult<HistoryReadMetricInfo[]> GetReadMetrics()
	{
		var user = Authenticate();
		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		return MetricsService.ReadMetrics();
	}
}
