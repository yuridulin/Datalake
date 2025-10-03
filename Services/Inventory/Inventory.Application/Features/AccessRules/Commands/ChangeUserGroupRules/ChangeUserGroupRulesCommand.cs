using Datalake.Inventory.Application.Features.AccessRules.Models;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Application.Entities;

namespace Datalake.Inventory.Application.Features.AccessRules.Commands.ChangeUserGroupRules;

public record ChangeUserGroupRulesCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public required Guid UserGroupGuid { get; init; }

	public required IEnumerable<ActorRuleDto> Rules { get; init; }
}
