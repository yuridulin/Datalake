using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.AccessRules.DTOs;

public record ActorRuleDto(
	AccessType Type,
	int? BlockId,
	int? SourceId,
	int? TagId);
