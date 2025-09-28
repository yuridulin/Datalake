using Datalake.InventoryService.Application.Interfaces;

namespace Datalake.InventoryService.Application.Features.Blocks.Commands.UpdateBlock;

public interface IUpdateBlockHandler : ICommandHandler<UpdateBlockCommand, int> { }
