using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Tags.Commands.DeleteTag;

public record DeleteTagCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public required int Id { get; init; }
}
