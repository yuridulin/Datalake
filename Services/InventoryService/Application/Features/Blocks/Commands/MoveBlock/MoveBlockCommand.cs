using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.Blocks.Commands.MoveBlock;

public record MoveBlockCommand(
	UserAccessEntity User,
	int BlockId,
	int? ParentId) : ICommandRequest;
