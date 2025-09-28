using Datalake.PublicApi.Models.AccessRights;

namespace Datalake.InventoryService.Application.Features.AccessRules.Queries.GetAccessRules;

public class GetAccessRulesQueryHandler : IGetAccessRulesQueryHandler
{
	public Task<IEnumerable<AccessRightsInfo>> HandleAsync(GetAccessRulesQuery query, CancellationToken ct = default)
	{
		throw new NotImplementedException();
	}
}
