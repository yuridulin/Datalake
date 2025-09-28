using Datalake.InventoryService.Application.Interfaces;
using Datalake.PublicApi.Models.AccessRights;

namespace Datalake.InventoryService.Application.Features.AccessRules.Queries.GetAccessRules;

public record GetAccessRulesQuery() : IQuery<IEnumerable<AccessRightsInfo>>;