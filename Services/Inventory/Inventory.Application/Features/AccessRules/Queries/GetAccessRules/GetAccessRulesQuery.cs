using Datalake.Contracts.Models.AccessRules;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.AccessRules.Queries.GetAccessRules;

public record GetAccessRulesQuery : IQueryRequest<List<AccessRightsInfo>>
{
	public Guid? UserGuid { get; init; }
	public Guid? UserGroupGuid { get; init; }
	public int? SourceId { get; init; }
	public int? BlockId { get; init; }
	public int? TagId { get; init; }
}
