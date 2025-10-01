using Datalake.InventoryService.Application.Features.AccessRules.Models;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeUserGroupRules;

public record ChangeUserGroupRulesCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public required Guid UserGroupGuid { get; init; }

	public required IEnumerable<ActorRuleDto> Rules { get; init; }
}
