namespace Datalake.Inventory.Application.Features.Tags.Models;

public record TagInputDto
{
	public required int TagId { get; init; }

	public required string VariableName { get; init; }

	public required int BlockId { get; init; }
}