using Datalake.InventoryService.Application.Interfaces;
using Datalake.PublicApi.Models.AccessRules;

namespace Datalake.InventoryService.Application.Features.AccessRules.Queries.GetAccessRules;

public record GetAccessRulesQuery : IQueryRequest<IEnumerable<AccessRightsInfo>>
{
	public Guid? UserGuid { get; init; }
	public Guid? UserGroupGuid { get; init; }
	public int? SourceId { get; init; }
	public int? BlockId { get; init; }
	public int? TagId { get; init; }
}
