using Datalake.Inventory.Application.Models;

namespace Datalake.Inventory.Application.Interfaces.InMemory;

public interface IUserAccessCalculationService
{
	UsersAccessDto CalculateAccess(IInventoryCacheState state);
}