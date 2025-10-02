using Datalake.Contracts.Public.Enums;

namespace Datalake.Inventory.Application.Features.AccessRules.Models;

public record ActorRuleDto
{
	public required AccessType Type { get; init; }
	public int? BlockId { get; init; }
	public int? SourceId { get; init; }
	public int? TagId { get; init; }
}
