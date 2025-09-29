using Datalake.InventoryService.Application.Features.AccessRules.DTOs;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeTagRules;

public record ChangeTagRulesCommand(
	UserAccessEntity User,
	int TagId,
	IEnumerable<ObjectRuleDto> Rules) : ICommand;
