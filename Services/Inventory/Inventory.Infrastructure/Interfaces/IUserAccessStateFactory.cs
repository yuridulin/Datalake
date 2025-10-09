using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Infrastructure.InMemory.UserAccess;

namespace Datalake.Inventory.Infrastructure.Interfaces;

public interface IUserAccessStateFactory
{
	UserAccessState Create(IInventoryCacheState state);
}