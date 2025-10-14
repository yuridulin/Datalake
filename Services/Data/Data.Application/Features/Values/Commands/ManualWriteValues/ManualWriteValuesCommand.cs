using Datalake.Data.Api.Models.Values;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.Values.Commands.ManualWriteValues;

public record ManualWriteValuesCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public required IEnumerable<ValueWriteRequest> Requests { get; init; }
}
