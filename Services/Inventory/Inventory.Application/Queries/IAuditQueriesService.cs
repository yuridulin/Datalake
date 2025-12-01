using Datalake.Contracts.Models.LogModels;
using Datalake.Domain.Enums;

namespace Datalake.Inventory.Application.Queries;

public interface IAuditQueriesService
{
	Task<List<LogInfo>> GetAsync(
		int? lastId = null,
		int? firstId = null,
		int? take = null,
		int? sourceId = null,
		int? blockId = null,
		int? tagId = null,
		Guid? userGuid = null,
		Guid? groupGuid = null,
		LogCategory[]? categories = null,
		LogType[]? types = null,
		Guid? authorGuid = null,
		CancellationToken ct = default);
}
