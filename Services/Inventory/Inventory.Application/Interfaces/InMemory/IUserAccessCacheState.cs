using Datalake.Shared.Domain.Entities;

namespace Datalake.Inventory.Application.Interfaces.InMemory;

public interface IUserAccessCacheState
{
	Dictionary<Guid, UserAccessEntity> GetAll();

	bool TryGet(Guid userIdOrEnergoId, out UserAccessEntity info);
}