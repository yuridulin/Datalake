using Datalake.InventoryService.Application.Interfaces;

namespace Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeUserGroupRules;

public interface IChangeUserGroupRulesCommandHandler : ICommandHandler<ChangeUserGroupRulesCommand, bool>
{
}
