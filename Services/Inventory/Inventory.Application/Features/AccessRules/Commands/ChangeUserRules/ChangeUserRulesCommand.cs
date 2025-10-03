using Datalake.Inventory.Application.Features.AccessRules.Models;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Application.Entities;

namespace Datalake.Inventory.Application.Features.AccessRules.Commands.ChangeUserRules;

public record ChangeUserRulesCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public required Guid UserGuid { get; init; }

	public required IEnumerable<ActorRuleDto> Rules { get; init; }
}
