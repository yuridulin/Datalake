using Datalake.ApiClasses.Constants;
using Datalake.Database.Extensions;
using Datalake.Database.Repositories.Base;

namespace Datalake.Database.Repositories;

public partial class SystemRepository(DatalakeContext db) : RepositoryBase
{
	public async Task<string> GetLastUpdateDate()
	{
		var lastUpdate = await db.GetLastUpdateAsync();
		return lastUpdate.ToString(DateFormats.HierarchicalWithMilliseconds);
	}
}
