using Datalake.InventoryService.Domain.Queries;
using Datalake.PrivateApi.Attributes;
using Datalake.PublicApi.Models.LogModels;

namespace Datalake.InventoryService.Application.Features.Audit.Queries.Audit;

[Scoped]
public class GetAuditQueryHandler(IAuditQueriesService auditQueriesService) : IGetAuditQueryHandler
{
	public Task<IEnumerable<LogInfo>> HandleAsync(GetAuditQuery query, CancellationToken ct = default)
	{
		return auditQueriesService.GetAsync(
			query.LastId,
			query.FirstId,
			query.Take,
			query.SourceId,
			query.BlockId,
			query.TagGuid,
			query.UserGuid,
			query.GroupGuid,
			query.Categories,
			query.Types,
			query.AuthorGuid);
	}
}
