using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Queries;
using Datalake.PrivateApi.Attributes;
using Datalake.PublicApi.Models.LogModels;

namespace Datalake.InventoryService.Application.Features.Audit.Queries.GetAudit;

public interface IGetAuditHandler : IQueryHandler<GetAuditQuery, IEnumerable<LogInfo>> { }

[Scoped]
public class GetAuditHandler(IAuditQueriesService auditQueriesService) : IGetAuditHandler
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
			query.AuthorGuid,
			ct);
	}
}
