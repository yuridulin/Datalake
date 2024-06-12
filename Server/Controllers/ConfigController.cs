using Datalake.ApiClasses.Constants;
using Datalake.ApiClasses.Models.Logs;
using Datalake.ApiClasses.Models.Settings;
using Datalake.Database.Repositories;
using Datalake.Server.Controllers.Base;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Server.Controllers;

/// <summary>
/// Представление системной информации о работе сервера
/// </summary>
/// <param name="systemRepository">Репозиторий</param>
[Route("api/[controller]")]
[ApiController]
public class ConfigController(SystemRepository systemRepository) : ApiControllerBase
{
	/// <summary>
	/// Получение даты последнего изменения структуры базы данных
	/// </summary>
	/// <returns>Дата в строковом виде</returns>
	[HttpGet("last")]
	public async Task<ActionResult<string>> GetLastUpdateAsync()
	{
		var lastUpdate = await systemRepository.GetLastUpdateDate();
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
		var query = systemRepository.GetLogs()
			.Where(x => !lastId.HasValue || x.Id > lastId.Value);

		if (take.HasValue)
			query = query.Take(take.Value);

		return await query
			.ToArrayAsync();
	}

	/// <summary>
	/// Получение информации о настройках сервера
	/// </summary>
	/// <returns>Информация о настройках</returns>
	[HttpGet("settings")]
	public async Task<ActionResult<SettingsInfo>> GetSettingsAsync()
	{
		var info = await systemRepository.GetSettingsAsync();

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

		await systemRepository.UpdateSettingsAsync(user, newSettings);

		return NoContent();
	}
}
