using Datalake.Inventory.Application.Features.AccessRules.Models;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.AccessRules.Commands.ChangeUserGroupRules;

public record ChangeUserGroupRulesCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public required Guid UserGroupGuid { get; init; }

	public required IEnumerable<ActorRuleDto> Rules { get; init; }
}
