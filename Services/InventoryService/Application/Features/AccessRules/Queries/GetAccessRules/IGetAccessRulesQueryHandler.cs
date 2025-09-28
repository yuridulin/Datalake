using Datalake.InventoryService.Application.Interfaces;
using Datalake.PublicApi.Models.AccessRights;

namespace Datalake.InventoryService.Application.Features.AccessRules.Queries.GetAccessRules;

public interface IGetAccessRulesQueryHandler : IQueryHandler<GetAccessRulesQuery, IEnumerable<AccessRightsInfo>>
{
}
