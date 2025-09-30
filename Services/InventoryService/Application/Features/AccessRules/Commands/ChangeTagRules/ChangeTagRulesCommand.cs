using Datalake.InventoryService.Application.Features.AccessRules.Models;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeTagRules;

public record ChangeTagRulesCommand(
	UserAccessEntity User,
	int TagId,
	IEnumerable<ObjectRuleDto> Rules) : ICommandRequest;
