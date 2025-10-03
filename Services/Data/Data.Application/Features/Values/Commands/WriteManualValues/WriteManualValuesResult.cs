namespace Datalake.Data.Application.Features.Values.Commands.WriteManualValues;

public record WriteManualValuesResult
{
	public required IEnumerable<int> Success { get; init; }

	public required IDictionary<int, string> Failures { get; init; }
}
