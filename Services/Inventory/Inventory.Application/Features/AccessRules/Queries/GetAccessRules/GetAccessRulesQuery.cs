using Datalake.Inventory.Api.Models.AccessRules;
using Datalake.Inventory.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.AccessRules.Queries.GetAccessRules;

public record GetAccessRulesQuery : IQueryRequest<IEnumerable<AccessRightsInfo>>
{
	public Guid? UserGuid { get; init; }
	public Guid? UserGroupGuid { get; init; }
	public int? SourceId { get; init; }
	public int? BlockId { get; init; }
	public int? TagId { get; init; }
}
