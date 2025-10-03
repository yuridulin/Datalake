using Datalake.Data.Api.Models.Values;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.Values.Commands.WriteManualValues;

public record WriteManualValuesCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public required IEnumerable<ValueWriteRequest> Requests { get; init; }
}
