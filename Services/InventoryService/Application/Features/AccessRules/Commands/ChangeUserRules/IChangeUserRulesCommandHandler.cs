using Datalake.InventoryService.Application.Interfaces;

namespace Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeUserRules;

public interface IChangeUserRulesCommandHandler : ICommandHandler<ChangeUserRulesCommand, bool>
{
}
