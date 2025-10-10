using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Entities;
using Datalake.Domain.Interfaces;

namespace Datalake.Inventory.Application.Models;

public record UserGroupRelationMemoryDto : IWithIdentityKey
{
	public required int Id { get; init; }

	public required Guid UserGuid { get; init; }

	public required Guid UserGroupGuid { get; init; }

	public required AccessType AccessType { get; init; }

	public static UserGroupRelationMemoryDto FromEntity(UserGroupRelation userGroupRelation)
	{
		return new()
		{
			Id = userGroupRelation.Id,
			AccessType = userGroupRelation.AccessType,
			UserGuid = userGroupRelation.UserGuid,
			UserGroupGuid = userGroupRelation.UserGroupGuid,
		};
	}
}
