using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Models;

namespace Datalake.Inventory.Application.Interfaces;

public interface IUserAccessCalculationService
{
	UsersAccessDto CalculateAccess(IInventoryCacheState state);
}