using Datalake.InventoryService.Infrastructure.Cache.UserAccess;

namespace Datalake.InventoryService.Application.Interfaces.InMemory;

public interface IUserAccessCache
{
	UserAccessState State { get; }

	event EventHandler<UserAccessState>? StateChanged;
}