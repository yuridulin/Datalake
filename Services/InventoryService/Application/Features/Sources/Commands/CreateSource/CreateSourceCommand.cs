using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.Sources.Commands.CreateSource;

public record CreateSourceCommand : ICommandRequest
{
	public required UserAccessEntity User { get; init; }
	public string? Name { get; init; }
	public string? Description { get; init; }
	public string? Address { get; init; }
	public SourceType? Type  { get; init; }
}
