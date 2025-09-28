using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeBlockRules;

public record ChangeBlockRulesCommand(
	UserAccessEntity User,
	int BlockId) : ICommand;
