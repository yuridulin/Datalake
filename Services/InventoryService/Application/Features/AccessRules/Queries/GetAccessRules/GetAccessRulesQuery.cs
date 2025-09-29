using Datalake.InventoryService.Application.Interfaces;
using Datalake.PublicApi.Models.AccessRights;

namespace Datalake.InventoryService.Application.Features.AccessRules.Queries.GetAccessRules;

public record GetAccessRulesQuery(
	Guid? UserGuid = null,
	Guid? UserGroupGuid = null,
	int? SourceId = null,
	int? BlockId = null,
	int? TagId = null) : IQuery<IEnumerable<AccessRightsInfo>>;
