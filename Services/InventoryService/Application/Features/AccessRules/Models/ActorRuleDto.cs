using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.AccessRules.Models;

public record ActorRuleDto(
	AccessType Type,
	int? BlockId,
	int? SourceId,
	int? TagId);
