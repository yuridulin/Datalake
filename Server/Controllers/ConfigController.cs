using Datalake.ApiClasses.Constants;
using Datalake.ApiClasses.Models.Logs;
using Datalake.ApiClasses.Models.Settings;
using Datalake.Database;
using Datalake.Database.Repositories;
using Datalake.Server.BackgroundServices.SettingsHandler;
using Datalake.Server.Controllers.Base;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Server.Controllers;

/// <summary>
/// Представление системной информации о работе сервера
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ConfigController(
	DatalakeContext db,
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
	/// <returns></returns>
	[HttpGet("logs")]
	public async Task<ActionResult<LogInfo[]>> GetLogsAsync(
		[FromQuery] int? take,
		[FromQuery] int? lastId)
	{
		var query = db.SystemRepository.GetLogs()
			.Where(x => !lastId.HasValue || x.Id > lastId.Value);

		if (take.HasValue)
			query = query.Take(take.Value);

		return await query
			.OrderByDescending(x => x.Id)
			.ToArrayAsync();
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
