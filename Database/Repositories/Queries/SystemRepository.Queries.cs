using Datalake.ApiClasses.Constants;
using Datalake.ApiClasses.Models.Logs;
using LinqToDB;

namespace Datalake.Database.Repositories;

public partial class SystemRepository
{
	public IQueryable<LogInfo> GetLogs()
	{
		var query = db.Logs
			.Select(x => new LogInfo
			{
				Id = x.Id,
				Category = x.Category,
				DateString = x.Date.ToString(DateFormats.Standart),
				Text = x.Text,
				Type = x.Type,
				RefId = x.RefId,
			});

		return query;
	}
}
