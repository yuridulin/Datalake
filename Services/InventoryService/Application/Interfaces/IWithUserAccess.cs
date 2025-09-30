using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Interfaces;

public interface IWithUserAccess
{
	UserAccessEntity User { get; init; }
}
