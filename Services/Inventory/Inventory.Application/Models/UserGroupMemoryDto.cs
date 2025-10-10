using Datalake.Domain.Entities;
using Datalake.Domain.Interfaces;

namespace Datalake.Inventory.Application.Models;

public record UserGroupMemoryDto : IWithGuidKey
{
	public required Guid Guid { get; init; }

	public required Guid? ParentGuid { get; init; }

	public static UserGroupMemoryDto FromEntity(UserGroup userGroup)
	{
		return new()
		{
			Guid = userGroup.Guid,
			ParentGuid = userGroup.ParentGuid,
		};
	}
}
