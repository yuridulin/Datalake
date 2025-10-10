using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Entities;
using Datalake.Domain.Interfaces;

namespace Datalake.Inventory.Application.Models;

public record AccessRightsMemoryDto : IWithIdentityKey
{
	public required int Id { get; init; }

	public required AccessType AccessType { get; init; }

	public required bool IsGlobal { get; init; }

	public required Guid? UserGuid { get; init; }

	public required Guid? UserGroupGuid { get; init; }

	public required int? SourceId { get; init; }

	public required int? BlockId { get; init; }

	public required int? TagId { get; init; }

	public static AccessRightsMemoryDto FromEntity(AccessRights rule)
	{
		return new()
		{
			Id = rule.Id,
			IsGlobal = rule.IsGlobal,
			AccessType = rule.AccessType,
			UserGuid = rule.UserGuid,
			UserGroupGuid = rule.UserGroupGuid,
			SourceId = rule.SourceId,
			BlockId = rule.BlockId,
			TagId = rule.TagId,
		};
	}
}
