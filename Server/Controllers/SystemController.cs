using Datalake.Database;
using Datalake.Database.Constants;
using Datalake.Database.Enums;
using Datalake.Database.Models.Logs;
using Datalake.Database.Models.Settings;
using Datalake.Database.Repositories;
using Datalake.Server.BackgroundServices.SettingsHandler;
using Datalake.Server.Controllers.Base;
using Datalake.Server.Models.System;
using Datalake.Server.Services.StateManager;
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
	/// <param name="take">Сколько сообщений получить за этот запрос</param>
	/// <param name="lastId">Идентификатор сообщения, с которого начать отсчёт количества</param>
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
		[FromQuery] int? take = null,
		[FromQuery] int? lastId = null,
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

		return await db.SystemRepository.GetLogsAsync(userAuth, take, lastId, source, block, tag, user, group, categories, types, author);
	}

	/// <summary>
	/// Информация о визитах пользователей
	/// </summary>
	/// <returns>Даты визитов, сопоставленные с идентификаторами пользователей</returns>
	[HttpGet("visits")]
	public ActionResult<Dictionary<Guid, DateTime>> GetVisits()
	{
		var user = Authenticate();

		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.User);

		return usersStateService.State;
	}

	/// <summary>
	/// Информация о подключении к источникам данных
	/// </summary>
	/// <returns></returns>
	[HttpGet("sources")]
	public ActionResult<Dictionary<int, SourceState>> GetSources()
	{
		var user = Authenticate();

		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Viewer);

		return sourcesStateService.State
			.Where(x => AccessRepository.HasAccessToSource(user, AccessType.Viewer, x.Key))
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

		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.User);

		var info = await db.SystemRepository.GetSettingsAsync(user);

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

		await db.SystemRepository.UpdateSettingsAsync(user, newSettings);
		await settingsService.WriteStartipFileAsync(db.SystemRepository);

		return NoContent();
	}

	/// <summary>
	/// Перестроение кэша и перезапуск всех сборщиков
	/// </summary>
	/// <returns></returns>
	[HttpPut("restart")]
	public async Task<ActionResult> RestartAsync()
	{
		var user = Authenticate();

		await db.SystemRepository.RebuildStorageCacheAsync(user);

		return NoContent();
	}
}
