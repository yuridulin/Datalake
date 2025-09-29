using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Domain.Queries;
using Datalake.PublicApi.Models.AccessRights;

namespace Datalake.InventoryService.Application.Features.AccessRules.Queries.GetAccessRules;

public interface IGetAccessRulesHandler : IQueryHandler<GetAccessRulesQuery, IEnumerable<AccessRightsInfo>> { }

public class GetAccessRulesHandler(
	IAccessRulesQueriesService accessRulesQueriesService) : IGetAccessRulesHandler
{
	public async Task<IEnumerable<AccessRightsInfo>> HandleAsync(GetAccessRulesQuery query, CancellationToken ct = default)
	{
		var data = await accessRulesQueriesService.GetAsync(query.UserGuid, query.UserGroupGuid, query.SourceId, query.BlockId, query.TagId);

		return data;
	}
}
