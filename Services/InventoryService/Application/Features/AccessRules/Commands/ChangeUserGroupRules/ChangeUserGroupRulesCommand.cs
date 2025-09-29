using Datalake.InventoryService.Application.Features.AccessRules.DTOs;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeUserGroupRules;

public record ChangeUserGroupRulesCommand(
	UserAccessEntity User,
	Guid UserGroupGuid,
	IEnumerable<ActorRuleDto> Rules) : ICommand;
