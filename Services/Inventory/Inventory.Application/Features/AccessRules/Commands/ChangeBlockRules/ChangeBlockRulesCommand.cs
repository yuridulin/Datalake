using Datalake.Inventory.Application.Features.AccessRules.Models;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.AccessRules.Commands.ChangeBlockRules;

public record ChangeBlockRulesCommand(
	UserAccessEntity User,
	int BlockId,
	IEnumerable<ObjectRuleDto> Rules) : ICommandRequest;
