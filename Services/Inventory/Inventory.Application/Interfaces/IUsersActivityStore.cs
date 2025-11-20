namespace Datalake.Inventory.Application.Interfaces;

public interface IUsersActivityStore
{
	void Set(Guid userGuid);

	IDictionary<string, DateTime> Get(IEnumerable<Guid> usersGuid);
}
