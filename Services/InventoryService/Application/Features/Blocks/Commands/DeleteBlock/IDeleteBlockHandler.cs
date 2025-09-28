using Datalake.InventoryService.Application.Interfaces;

namespace Datalake.InventoryService.Application.Features.Blocks.Commands.DeleteBlock;

public interface IDeleteBlockHandler : ICommandHandler<DeleteBlockCommand, bool> { }
