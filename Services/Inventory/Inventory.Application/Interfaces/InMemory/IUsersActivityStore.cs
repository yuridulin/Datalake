namespace Datalake.Inventory.Application.Interfaces.InMemory;

public interface IUsersActivityStore
{
	void Set(Guid userGuid);

	IDictionary<string, DateTime> Get(IEnumerable<Guid> usersGuid);
}
