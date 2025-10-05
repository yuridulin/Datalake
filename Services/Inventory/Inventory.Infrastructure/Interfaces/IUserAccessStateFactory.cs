using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Infrastructure.Cache.UserAccess;

namespace Datalake.Inventory.Infrastructure.Interfaces;

public interface IUserAccessStateFactory
{
	UserAccessState Create(IInventoryCacheState state);
}