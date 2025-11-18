using Datalake.Gateway.Application.Interfaces;
using System.Collections.Concurrent;

namespace Datalake.Gateway.Infrastructure.InMemory;

public class UsersActivityService : IUsersActivityService
{
	private ConcurrentDictionary<Guid, DateTime> _state = [];

	public DateTime? Get(Guid userGuid)
	{
		return _state.TryGetValue(userGuid, out var date) ? date : null;
	}

	public void Set(Guid userGuid)
	{
		_state.AddOrUpdate(
			userGuid,
			(_) => DateTime.UtcNow,
			(_, _) => DateTime.UtcNow);
	}
}
