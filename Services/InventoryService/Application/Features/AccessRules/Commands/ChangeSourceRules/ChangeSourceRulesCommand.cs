using Datalake.InventoryService.Application.Features.AccessRules.Models;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeSourceRules;

public record ChangeSourceRulesCommand(
	UserAccessEntity User,
	int SourceId,
	IEnumerable<ObjectRuleDto> Rules) : ICommandRequest;
