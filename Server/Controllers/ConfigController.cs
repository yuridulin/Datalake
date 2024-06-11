using DatalakeApiClasses.Constants;
using DatalakeApiClasses.Models.Logs;
using DatalakeDatabase;
using DatalakeDatabase.Extensions;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;

namespace DatalakeServer.Controllers;

/// <summary>
/// Представление системной информации о работе сервера
/// </summary>
/// <param name="db">Экземпляр подключения к базе данных</param>
[Route("api/[controller]")]
[ApiController]
public class ConfigController(DatalakeContext db) : ControllerBase
{
	/// <summary>
	/// Получение даты последнего изменения структуры базы данных
	/// </summary>
	/// <returns>Дата в строковом виде</returns>
	[HttpGet("last")]
	public async Task<ActionResult<string>> GetLastUpdateAsync()
	{
		var lastUpdate = await db.GetLastUpdateAsync();
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
		var query = db.Logs
			.Where(x => !lastId.HasValue || x.Id > lastId.Value);

		if (take.HasValue)
			query = query.Take(take.Value);

		return await query
			.Select(x => new LogInfo
			{
				Id = x.Id,
				Category = x.Category,
				DateString = x.Date.ToString(DateFormats.Standart),
				Text = x.Text,
				Type = x.Type,
				RefId = x.RefId,
			})
			.ToArrayAsync();
	}
}
