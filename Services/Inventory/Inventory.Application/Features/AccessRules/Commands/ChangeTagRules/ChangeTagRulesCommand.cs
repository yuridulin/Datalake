using Datalake.Inventory.Application.Features.AccessRules.Models;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Application.Entities;

namespace Datalake.Inventory.Application.Features.AccessRules.Commands.ChangeTagRules;

public record ChangeTagRulesCommand(
	UserAccessEntity User,
	int TagId,
	IEnumerable<ObjectRuleDto> Rules) : ICommandRequest;
