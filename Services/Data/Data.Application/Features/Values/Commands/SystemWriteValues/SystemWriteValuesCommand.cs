using Datalake.Domain.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.Values.Commands.SystemWriteValues;

public record SystemWriteValuesCommand : ICommandRequest
{
	public required IEnumerable<TagValue> Values { get; init; }
}
