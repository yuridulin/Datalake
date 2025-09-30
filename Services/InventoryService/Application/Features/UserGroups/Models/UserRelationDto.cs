using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.UserGroups.Models;

public record UserRelationDto
{
	public required Guid Guid { get; init; }

	public required AccessType AccessType { get; init; }
}
