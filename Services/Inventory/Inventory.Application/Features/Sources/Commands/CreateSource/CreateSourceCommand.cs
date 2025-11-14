using Datalake.Domain.Enums;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Sources.Commands.CreateSource;

public record CreateSourceCommand : ICommandRequest
{
	public required UserAccessValue User { get; init; }
	public string? Name { get; init; }
	public string? Description { get; init; }
	public string? Address { get; init; }
	public SourceType? Type { get; init; }
}
