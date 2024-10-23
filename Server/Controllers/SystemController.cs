using Datalake.ApiClasses.Constants;
using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Models.Logs;
using Datalake.ApiClasses.Models.Settings;
using Datalake.Database;
using Datalake.Database.Repositories;
using Datalake.Server.BackgroundServices.SettingsHandler;
using Datalake.Server.Controllers.Base;
using Datalake.Server.Models.System;
using Datalake.Server.Services.StateManager;
using LinqToDB;
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
		var query = db.SystemRepository.GetLogs();
		if (lastId.HasValue)
			query = query.Where(x => x.Id > lastId.Value);

		if (source.HasValue)
			query = query.Where(x => x.RefId == source.Value.ToString());

		if (block.HasValue)
			query = query.Where(x => x.RefId == block.Value.ToString());

		if (tag.HasValue)
			query = query.Where(x => x.RefId == tag.Value.ToString());

		if (user.HasValue)
			query = query.Where(x => x.RefId == user.Value.ToString());

		if (group.HasValue)
			query = query.Where(x => x.RefId == group.Value.ToString());

		if (categories != null && categories.Length > 0)
			query = query.Where(x => categories.Contains(x.Category));

		if (types != null && types.Length > 0)
			query = query.Where(x =>  types.Contains(x.Type));
		
		if (author != null)
			query = query.Where(x => x.Author != null && x.Author.Guid == author.Value);

		if (take.HasValue)
			query = query.Take(take.Value);

		return await query
			.OrderByDescending(x => x.Id)
			.ToArrayAsync();
	}

	/// <summary>
	/// Информация о визитах пользователей
	/// </summary>
	/// <returns>Даты визитов, сопоставленные с идентификаторами пользователей</returns>
	[HttpGet("visits")]
	public ActionResult<Dictionary<Guid, DateTime>> GetVisits()
	{
		return usersStateService.State;
	}

	/// <summary>
	/// Информация о подключении к источникам данных
	/// </summary>
	/// <returns></returns>
	[HttpGet("sources")]
	public ActionResult<Dictionary<int, SourceState>> GetSources()
	{
		return sourcesStateService.State;
	}

	/// <summary>
	/// Получение информации о настройках сервера
	/// </summary>
	/// <returns>Информация о настройках</returns>
	[HttpGet("settings")]
	public async Task<ActionResult<SettingsInfo>> GetSettingsAsync()
	{
		var info = await db.SystemRepository.GetSettingsAsync();

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

		await db.SystemRepository.RebuildCacheAsync(user);
		SystemRepository.Update();

		return NoContent();
	}
}
