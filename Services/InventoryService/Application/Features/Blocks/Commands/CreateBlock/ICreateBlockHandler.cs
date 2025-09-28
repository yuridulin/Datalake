using Datalake.InventoryService.Application.Interfaces;

namespace Datalake.InventoryService.Application.Features.Blocks.Commands.CreateBlock;

public interface ICreateBlockHandler : ICommandHandler<CreateBlockCommand, int> { }
