using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Entities;
using Datalake.Domain.Interfaces;

namespace Datalake.Inventory.Application.Models;

public class UserMemoryDto : IWithGuidKey
{
	public required Guid Guid { get; set; }

	public required bool IsEnergoId { get; set; }

	public static UserMemoryDto FromEntity(User user)
	{
		return new()
		{
			Guid = user.Guid,
			IsEnergoId = user.Type == UserType.EnergoId,
		};
	}
}
