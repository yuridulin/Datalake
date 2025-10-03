using Datalake.Inventory.Application.Queries;
using Datalake.Inventory.Api.Models.AccessRules;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.AccessRules.Queries.GetAccessRules;

public interface IGetAccessRulesHandler : IQueryHandler<GetAccessRulesQuery, IEnumerable<AccessRightsInfo>> { }

public class GetAccessRulesHandler(
	IAccessRulesQueriesService accessRulesQueriesService) : IGetAccessRulesHandler
{
	public async Task<IEnumerable<AccessRightsInfo>> HandleAsync(GetAccessRulesQuery query, CancellationToken ct = default)
	{
		var data = await accessRulesQueriesService.GetAsync(query.UserGuid, query.UserGroupGuid, query.SourceId, query.BlockId, query.TagId, ct);

		return data;
	}
}
