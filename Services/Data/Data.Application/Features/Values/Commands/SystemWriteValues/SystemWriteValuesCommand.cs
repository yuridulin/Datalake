using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.Values.Commands.SystemWriteValues;

public record SystemWriteValuesCommand : ICommandRequest
{
	public required IEnumerable<TagHistoryValue> Values { get; init; }
}
