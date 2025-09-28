using Datalake.InventoryService.Application.Interfaces;

namespace Datalake.InventoryService.Application.Features.Blocks.Commands.UpdateBlockTags;

public interface IUpdateBlockTagsHandler : ICommandHandler<UpdateBlockTagsCommand, int> { }
