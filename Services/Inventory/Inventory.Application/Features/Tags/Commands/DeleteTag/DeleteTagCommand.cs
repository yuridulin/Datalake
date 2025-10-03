using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Application.Entities;

namespace Datalake.Inventory.Application.Features.Tags.Commands.DeleteTag;

public record DeleteTagCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public required int Id { get; init; }
}
