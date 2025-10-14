using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.DataCollection.Commands.ManualRestartCollection;

public record ManualRestartCollectionCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessValue User { get; init; }
}
