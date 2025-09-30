using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.AccessRules.Models;

public record ObjectRuleDto(
	AccessType Type,
	Guid? UserGuid,
	Guid? UserGroupGuid);
