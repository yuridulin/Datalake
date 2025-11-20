using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Application.Interfaces.AccessRules;
using System.Collections.Concurrent;

namespace Datalake.Shared.Infrastructure.InMemory;

[Singleton]
public class UsersAccessStore : IUsersAccessStore
{
	private ConcurrentDictionary<Guid, UserAccessValue> store = [];

	public UserAccessValue? Get(Guid userGuid)
	{
		if (!store.TryGetValue(userGuid, out var access))
			return null;

		return access;
	}

	public void Set(IDictionary<Guid, UserAccessValue> access)
	{
		foreach (var (userGuid, accessValue) in access)
		{
			store.AddOrUpdate(userGuid, _ => accessValue, (_, _) => accessValue);
		}
	}
}
