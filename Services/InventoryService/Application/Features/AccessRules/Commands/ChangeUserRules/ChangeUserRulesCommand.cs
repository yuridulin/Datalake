using Datalake.InventoryService.Application.Features.AccessRules.Models;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeUserRules;

public record ChangeUserRulesCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public required Guid UserGuid { get; init; }

	public required IEnumerable<ActorRuleDto> Rules { get; init; }
}
