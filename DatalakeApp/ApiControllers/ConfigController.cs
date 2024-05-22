using DatalakeDatabase;
using DatalakeDatabase.ApiModels.Logs;
using DatalakeDatabase.Constants;
using DatalakeDatabase.Extensions;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;

namespace DatalakeApp.ApiControllers;

[Route("api/[controller]")]
[ApiController]
public class ConfigController(DatalakeContext db) : ControllerBase
{
	[HttpGet("last")]
	public async Task<ActionResult<string>> GetLastUpdateAsync()
	{
		var lastUpdate = await db.GetLastUpdateAsync();
		return lastUpdate.ToString(DateFormats.HierarchicalWithMilliseconds);
	}

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
