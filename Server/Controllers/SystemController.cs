using Datalake.Database;
using Datalake.Database.Functions;
using Datalake.Database.InMemory;
using Datalake.Database.InMemory.Repositories;
using Datalake.Database.Repositories;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.LogModels;
using Datalake.PublicApi.Models.Settings;
using Datalake.Server.Controllers.Base;
using Datalake.Server.Services.Maintenance;
using Datalake.Server.Services.Maintenance.Models;
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
	DatalakeDataStore dataStore,
	DatalakeDerivedDataStore derivedDataStore,
	DatalakeCurrentValuesStore valuesStore,
	SourcesStateService sourcesStateService,
	TagsStateService tagsStateService,
	UsersStateService usersStateService,
	SettingsMemoryRepository settingsRepository) : ApiControllerBase(derivedDataStore)
{
	/// <summary>
	/// Получение даты последнего изменения структуры базы данных
	/// </summary>
	/// <returns>Дата в строковом виде</returns>
	[HttpGet("last")]
	public ActionResult<string> GetLastUpdate()
	{
		var lastUpdate = dataStore.State.Version;
		return lastUpdate.ToString();
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

		return await AuditRepository.ReadAsync(db, userAuth, lastId, firstId, take, source, block, tag, user, group, categories, types, author);
	}

	/// <summary>
	/// Информация о визитах пользователей
	/// </summary>
	/// <returns>Даты визитов, сопоставленные с идентификаторами пользователей</returns>
	[HttpGet("visits")]
	public ActionResult<Dictionary<Guid, DateTime>> GetVisits()
	{
		var user = Authenticate();

		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Viewer);

		return usersStateService.State();
	}

	/// <summary>
	/// Информация о подключении к источникам данных
	/// </summary>
	/// <returns></returns>
	[HttpGet("sources")]
	public ActionResult<Dictionary<int, SourceState>> GetSourcesStates()
	{
		var user = Authenticate();

		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Viewer);

		return sourcesStateService.State()
			.Where(x => AccessChecks.HasAccessToSource(user, AccessType.Viewer, x.Key))
			.ToDictionary();
	}

	/// <summary>
	/// Информация о подключении к источникам данных
	/// </summary>
	/// <returns></returns>
	[HttpGet("tags")]
	public ActionResult<Dictionary<int, Dictionary<string, DateTime>>> GetTagsStates()
	{
		var user = Authenticate();

		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Viewer);

		return tagsStateService.GetTagsStates()
			.Where(x => AccessChecks.HasAccessToTag(user, AccessType.Viewer, x.Key))
			.ToDictionary();
	}

	/// <summary>
	/// Получение информации о настройках сервера
	/// </summary>
	/// <returns>Информация о настройках</returns>
	[HttpGet("settings")]
	public ActionResult<SettingsInfo> GetSettings()
	{
		var user = Authenticate();

		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Editor);

		var info = settingsRepository.GetSettings(user);

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

		await settingsRepository.UpdateSettingsAsync(db, user, newSettings);

		return NoContent();
	}

	/// <summary>
	/// Перестроение кэша
	/// </summary>
	/// <returns></returns>
	[HttpPut("restart/state")]
	public async Task<ActionResult> RestartStateAsync()
	{
		var user = Authenticate();

		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		await dataStore.ReloadStateAsync();

		return NoContent();
	}

	/// <summary>
	/// Перестроение кэша текущих (последних) значений
	/// </summary>
	/// <returns></returns>
	[HttpPut("restart/values")]
	public async Task<ActionResult> RestartValuesAsync()
	{
		var user = Authenticate();

		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		await valuesStore.ReloadValuesAsync();

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

		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		return DerivedDataStore.Access.GetAll();
	}
}
