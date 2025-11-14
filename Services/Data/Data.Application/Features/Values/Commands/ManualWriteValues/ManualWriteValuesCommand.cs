using Datalake.Contracts.Models.Data.Values;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.Values.Commands.ManualWriteValues;

public record ManualWriteValuesCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public required IEnumerable<ValueWriteRequest> Requests { get; init; }
}
