using Datalake.InventoryService.Application.Interfaces;

namespace Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeTagRules;

public interface IChangeTagRulesCommandHandler : ICommandHandler<ChangeTagRulesCommand, bool>
{
}
