using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.LogModels;

namespace Datalake.InventoryService.Domain.Queries;

public interface IAuditQueriesService
{
	Task<IEnumerable<LogInfo>> GetAsync(
		int? lastId = null,
		int? firstId = null,
		int? take = null,
		int? sourceId = null,
		int? blockId = null,
		Guid? tagGuid = null,
		Guid? userGuid = null,
		Guid? groupGuid = null,
		LogCategory[]? categories = null,
		LogType[]? types = null,
		Guid? authorGuid = null,
		CancellationToken ct = default);
}
