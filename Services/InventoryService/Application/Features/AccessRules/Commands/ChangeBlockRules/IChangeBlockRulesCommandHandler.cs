using Datalake.InventoryService.Application.Interfaces;

namespace Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeBlockRules;

public interface IChangeBlockRulesCommandHandler : ICommandHandler<ChangeBlockRulesCommand, bool>
{
}
