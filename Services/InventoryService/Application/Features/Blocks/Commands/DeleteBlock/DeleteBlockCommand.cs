using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.Blocks.Commands.DeleteBlock;

public record DeleteBlockCommand(
	UserAccessEntity User,
	int BlockId) : ICommand;
