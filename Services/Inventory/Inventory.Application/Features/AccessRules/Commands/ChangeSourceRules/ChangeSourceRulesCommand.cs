using Datalake.Domain.ValueObjects;
using Datalake.Inventory.Application.Features.AccessRules.Models;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.AccessRules.Commands.ChangeSourceRules;

public record ChangeSourceRulesCommand(
	UserAccessValue User,
	int SourceId,
	IEnumerable<ObjectRuleDto> Rules) : ICommandRequest;
