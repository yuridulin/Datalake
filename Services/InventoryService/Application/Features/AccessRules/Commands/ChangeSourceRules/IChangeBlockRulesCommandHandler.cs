using Datalake.InventoryService.Application.Interfaces;

namespace Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeSourceRules;

public interface IChangeSourceRulesCommandHandler : ICommandHandler<ChangeSourceRulesCommand, bool>
{
}
