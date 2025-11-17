using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Commands.CreateBlock;

public record CreateBlockCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessValue User { get; init; }
	public required int? ParentId { get; init; }
	public required string? Name { get; init; }
	public required string? Description { get; init; }
}
