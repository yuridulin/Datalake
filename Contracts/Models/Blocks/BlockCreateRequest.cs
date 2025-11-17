namespace Datalake.Contracts.Models.Blocks;

public record BlockCreateRequest
{
	public int? ParentId { get; init; }

	public string? Name { get; init; }

	public string? Description { get; init; }
}
