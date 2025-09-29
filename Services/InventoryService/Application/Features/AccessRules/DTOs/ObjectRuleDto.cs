using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.AccessRules.DTOs;

public record ObjectRuleDto(
	AccessType Type,
	Guid? UserGuid,
	Guid? UserGroupGuid);
