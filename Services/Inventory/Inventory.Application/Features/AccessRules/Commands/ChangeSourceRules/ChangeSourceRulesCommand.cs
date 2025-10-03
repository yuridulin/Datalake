using Datalake.Inventory.Application.Features.AccessRules.Models;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Application.Entities;

namespace Datalake.Inventory.Application.Features.AccessRules.Commands.ChangeSourceRules;

public record ChangeSourceRulesCommand(
	UserAccessEntity User,
	int SourceId,
	IEnumerable<ObjectRuleDto> Rules) : ICommandRequest;
