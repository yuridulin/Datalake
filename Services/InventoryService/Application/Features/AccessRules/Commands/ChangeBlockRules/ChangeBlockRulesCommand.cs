using Datalake.InventoryService.Application.Features.AccessRules.Models;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeBlockRules;

public record ChangeBlockRulesCommand(
	UserAccessEntity User,
	int BlockId,
	IEnumerable<ObjectRuleDto> Rules) : ICommandRequest;
