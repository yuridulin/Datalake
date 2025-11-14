using Datalake.Domain.Enums;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Sources.Commands.UpdateSource;

public record UpdateSourceCommand : ICommandRequest
{
	public required UserAccessValue User { get; init; }
	public required int SourceId { get; init; }
	public required string Name { get; init; }
	public string? Description { get; init; }
	public string? Address { get; init; }
	public required SourceType Type { get; init; }
	public required bool IsDisabled { get; init; }
}
