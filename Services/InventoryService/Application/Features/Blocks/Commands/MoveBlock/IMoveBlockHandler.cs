using Datalake.InventoryService.Application.Interfaces;

namespace Datalake.InventoryService.Application.Features.Blocks.Commands.MoveBlock;

public interface IMoveBlockHandler : ICommandHandler<MoveBlockCommand, int> { }
