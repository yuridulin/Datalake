using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.Blocks.Commands.UpdateBlock;

public record UpdateBlockCommand(
	UserAccessEntity User,
	int BlockId,
	string Name,
	string? Description) : ICommand;
