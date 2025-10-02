using Datalake.Inventory.Application.Features.AccessRules.Models;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Domain.Entities;

namespace Datalake.Inventory.Application.Features.AccessRules.Commands.ChangeBlockRules;

public record ChangeBlockRulesCommand(
	UserAccessEntity User,
	int BlockId,
	IEnumerable<ObjectRuleDto> Rules) : ICommandRequest;
