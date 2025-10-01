namespace Datalake.InventoryService.Application.Features.Tags.Models;

public record TagCalculationDto
{
	public string? Formula { get; init; }

	public IEnumerable<TagInputDto> FormulaInputs { get; init; } = [];
}
