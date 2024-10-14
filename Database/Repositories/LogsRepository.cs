using Datalake.Database.Models;
using LinqToDB;

namespace Datalake.Database.Repositories;

public class LogsRepository(DatalakeContext db)
{
	public async Task LogAsync(Log log)
	{
		await db.InsertAsync(log);
	}
}
