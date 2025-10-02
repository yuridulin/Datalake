using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Domain.Entities;

namespace Datalake.Inventory.Application.Features.Sources.Commands.CreateSource;

public record CreateSourceCommand : ICommandRequest
{
	public required UserAccessEntity User { get; init; }
	public string? Name { get; init; }
	public string? Description { get; init; }
	public string? Address { get; init; }
	public SourceType? Type  { get; init; }
}
