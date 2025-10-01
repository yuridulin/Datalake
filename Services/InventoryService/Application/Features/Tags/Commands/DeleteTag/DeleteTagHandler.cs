using Datalake.InventoryService.Application.Features.Tags.Commands.CreateTag;
using Datalake.InventoryService.Application.Interfaces;

namespace Datalake.InventoryService.Application.Features.Tags.Commands.DeleteTag;

public interface IDeleteTagHandler : ICommandHandler<DeleteTagCommand, bool> { }

public class DeleteTagHandler
{
}
