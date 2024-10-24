using Datalake.Database.Constants;
using Datalake.Database.Models.Logs;
using Datalake.Database.Models.Users;
using LinqToDB;

namespace Datalake.Database.Repositories;

public partial class SystemRepository
{
	public IQueryable<LogInfo> GetLogs()
	{
		var query =
			from log in db.Logs
			from user in db.Users.LeftJoin(x => x.Guid == log.UserGuid)
			select new LogInfo
			{
				Id = log.Id,
				Category = log.Category,
				DateString = log.Date.ToString(DateFormats.Standart),
				Text = log.Text,
				Type = log.Type,
				RefId = log.RefId,
				Author = user == null ? null : new UserSimpleInfo
				{
					Guid = user.Guid,
					FullName = user.FullName ?? user.Login ?? string.Empty,
				}
			};

		return query;
	}
}
