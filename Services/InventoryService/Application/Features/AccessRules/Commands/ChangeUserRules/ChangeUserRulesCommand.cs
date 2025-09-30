using Datalake.InventoryService.Application.Features.AccessRules.Models;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeUserRules;

public record ChangeUserRulesCommand(
	UserAccessEntity User,
	Guid UserGuid,
	IEnumerable<ActorRuleDto> Rules) : ICommandRequest;
