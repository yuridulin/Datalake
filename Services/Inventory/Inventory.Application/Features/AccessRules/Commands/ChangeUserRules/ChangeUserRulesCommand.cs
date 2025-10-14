using Datalake.Domain.ValueObjects;
using Datalake.Inventory.Application.Features.AccessRules.Models;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.AccessRules.Commands.ChangeUserRules;

public record ChangeUserRulesCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public required Guid UserGuid { get; init; }

	public required IEnumerable<ActorRuleDto> Rules { get; init; }
}
