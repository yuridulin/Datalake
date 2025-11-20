using Datalake.Inventory.Application.Models;

namespace Datalake.Inventory.Application.Interfaces;

public interface IUserAccessCalculationService
{
	UsersAccessDto CalculateAccess(IInventoryState state);
}