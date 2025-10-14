using Datalake.Inventory.Application.Features.AccessRules.Models;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.AccessRules.Commands.ChangeTagRules;

public record ChangeTagRulesCommand(
	UserAccessValue User,
	int TagId,
	IEnumerable<ObjectRuleDto> Rules) : ICommandRequest;
