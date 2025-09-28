namespace Datalake.InventoryService.Infrastructure.Cache.UserAccess;

public interface IUserAccessCache
{
	UserAccessState State { get; }

	event EventHandler<UserAccessState>? StateChanged;
}